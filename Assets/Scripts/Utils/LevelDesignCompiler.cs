using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class LevelDesignCompiler
{
    private const string DEBUG_PREFIX = "[LevelDetailsConverter] ";
    private const string COMMAND_SYMBOL = "#";
    private const string COMMENT_SYMBOL = "//";

    public static LevelDetails CompileToLevelDetails(string filePath)
    {
        Command currentCommand = null;
        bool searchForCommandName = false;
        bool isCommenting = false;

        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning($"{DEBUG_PREFIX} File path is null or empty.");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"{DEBUG_PREFIX} File does not exist at path: " + filePath);
            return null;
        }

        string fileContent = File.ReadAllText(filePath);
        var tokens = StringUtils.ParseString(fileContent);
        if (tokens == null)
        {
            Debug.Log($"{DEBUG_PREFIX} An error occurred, cannot parse into tokens!");
            return null;
        }

        Debug.Log($"{DEBUG_PREFIX} Extracted {tokens.Count} tokens");
        LevelDetails levelDetails = new();

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] == null)
            {
                continue;
            }

            Debug.Log($"{DEBUG_PREFIX} Processing token: {tokens[i]}");

            if (tokens[i] == COMMENT_SYMBOL)
            {
                Debug.Log($"{DEBUG_PREFIX} {(isCommenting ? "Stop" : "Start")} commenting");
                isCommenting = !isCommenting;
                continue;
            }

            // Skip all tokens when commenting
            if (isCommenting)
            {
                continue;
            }

            if (tokens[i] == COMMAND_SYMBOL)
            {
                Debug.Log($"{DEBUG_PREFIX} Command symbol detected");
                currentCommand = null;
                searchForCommandName = true;
            }
            else
            {
                if (searchForCommandName)
                {
                    // Only search for command name in the very first token that follows the command symbol
                    // If no valid command name is found -> Discard the command
                    currentCommand = CreateCommand(tokens[i], levelDetails);
                    if (currentCommand == null)
                    {
                        Debug.Log($"{DEBUG_PREFIX} Invalid command: {tokens[i]}");
                    }
                    else
                    {
                        Debug.Log($"{DEBUG_PREFIX} Find new command: {tokens[i]}");
                    }
                    searchForCommandName = false;
                }
                else
                {
                    currentCommand?.ProcessToken(tokens[i]);
                }
            }
        }

        return levelDetails;
    }

    private static Command CreateCommand(string command, LevelDetails levelDetails)
    {
        Command newCommand = null;
        switch (command)
        {
            case "LEVEL_CONFIG":
                newCommand = new LevelConfigCommand();
                break;
            case "CUBES":
                newCommand = new CubesCommand();
                break;
        }
        newCommand?.Init(levelDetails);
        return newCommand;
    }

    private interface Command
    {
        public void Init(LevelDetails levelDetails);
        public void ProcessToken(string token);
    }
    private class LevelConfigCommand : Command
    {
        private const string DEBUG_PREFIX = "[LevelConfigCommand] ";
        private static readonly HashSet<string> LEVEL_PROPERTIES = new()
        {
            "TIME"
        };

        private LevelDetails _levelDetails = null;
        private IValueParser _currentValueParser = null;
        private bool _searchForProperty = false;
        private string _propertyName;

        public void Init(LevelDetails levelDetails)
        {
            _levelDetails = levelDetails;
        }
        public void ProcessToken(string token)
        {
            Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (token == null)
            {
                Debug.Log($"{DEBUG_PREFIX} Invalid token!");
                return;
            }
            if (_levelDetails == null)
            {
                Debug.Log($"{DEBUG_PREFIX} Level details needs to be init!");
                return;
            }

            if (token == (COMMAND_SYMBOL + COMMAND_SYMBOL))
            {
                Debug.Log($"{DEBUG_PREFIX} Property command detected");
                _currentValueParser = null;
                _searchForProperty = true;
            }
            else
            {
                if (_searchForProperty)
                {
                    if (!IsValidProperty(token))
                    {
                        Debug.Log($"{DEBUG_PREFIX} Invalid property name: {token}");
                    }
                    else
                    {
                        Debug.Log($"{DEBUG_PREFIX} Chosen property: {token}");
                        _propertyName = token;
                        _currentValueParser = CreateValueParser(token);
                    }
                    _searchForProperty = false;
                }
                else if (_currentValueParser != null)
                {
                    _currentValueParser.ProcessToken(token);
                    if (_currentValueParser.Finished)
                    {
                        ApplyChange(_currentValueParser.Value);
                        _currentValueParser = null;
                    }
                }
            }
        }
        private bool IsValidProperty(string propertyName)
        {
            return LEVEL_PROPERTIES.Contains(propertyName);
        }
        private IValueParser CreateValueParser(string propertyName)
        {
            return propertyName switch
            {
                "TIME" => new SingleValueParser<float>(),
                _ => null,
            };
        }
        private void ApplyChange(object value)
        {
            if (_levelDetails == null)
            {
                Debug.Log($"{DEBUG_PREFIX} Cannot modify property when level details is null");
                return;
            }
            switch (_propertyName)
            {
                case "TIME":
                    if (value is float floatValue)
                    {
                        _levelDetails.Time = floatValue;
                    }
                    else
                    {
                        Debug.Log($"Receive invalid value type for {_propertyName}");
                    }
                    break;
            }
        }
    }
    private class CubesCommand : Command
    {
        private const string DEBUG_PREFIX = "[CubesCommand]";

        private bool _searchForCubeID = false;
        private LevelDetails _levelDetails = null;
        private CubeDetailsCommand _currentCubeDetailsCommand = null;

        public void Init(LevelDetails levelDetails)
        {
            _levelDetails = levelDetails;
        }

        public void ProcessToken(string token)
        {
            Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (token == null)
            {
                Debug.Log($"{DEBUG_PREFIX} Invalid token!");
                return;
            }
            if (_levelDetails == null)
            {
                Debug.Log($"{DEBUG_PREFIX} Level details needs to be init!");
                return;
            }

            if (token == (COMMAND_SYMBOL + COMMAND_SYMBOL))
            {
                Debug.Log($"{DEBUG_PREFIX} Cube details command detected");
                _searchForCubeID = true;
                _currentCubeDetailsCommand = null;
            }
            else
            {
                if (_searchForCubeID)
                {
                    string cubeID = token;
                    CubeDetails foundCubeDetails = null;

                    if (_levelDetails.CubeDetailsList != null)
                    {
                        foundCubeDetails = _levelDetails.CubeDetailsList.Find(cubeDetails => cubeDetails.CubeID == token);
                    }

                    if (foundCubeDetails == null)
                    {
                        foundCubeDetails = new()
                        {
                            CubeID = token
                        };
                        _levelDetails.CubeDetailsList ??= new();
                        _levelDetails.CubeDetailsList.Add(foundCubeDetails);
                    }
                    _currentCubeDetailsCommand = new(foundCubeDetails);
                    _searchForCubeID = false;
                }
                else
                {
                    _currentCubeDetailsCommand?.ProcessToken(token);
                }
            }
        }
        private class CubeDetailsCommand : Command
        {
            private const string DEBUG_PREFIX = "[CubeDetailsCommand]";
            private static readonly HashSet<string> LEVEL_PROPERTIES = new()
            {
                "COLOR_ID",
                "IS_PLAYER",
                "IS_FLIPPED",
                "IS_POSSESSABLE",
                "IS_SECONDARY_PLAYER",
                "CUBE_GRID"
            };

            private CubeDetails _cubeDetails = null;
            private IValueParser _valueParser = null;
            private bool _searchForProperty = false;
            private string _propertyName;
            public CubeDetailsCommand(CubeDetails targetCubeDetails)
            {
                _cubeDetails = targetCubeDetails;
            }

            public void Init(LevelDetails levelDetails) { }

            public void ProcessToken(string token)
            {
                Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

                if (token == null)
                {
                    Debug.Log($"{DEBUG_PREFIX} Invalid token!");
                    return;
                }
                if (_cubeDetails == null)
                {
                    Debug.Log($"{DEBUG_PREFIX} Cube details needs to be init!");
                    return;
                }

                if (token == (COMMAND_SYMBOL + COMMAND_SYMBOL + COMMAND_SYMBOL))
                {
                    Debug.Log($"{DEBUG_PREFIX} Property command detected");
                    _valueParser = null;
                    _searchForProperty = true;
                }
                else
                {
                    if (_searchForProperty)
                    {
                        if (!IsValidProperty(token))
                        {
                            Debug.Log($"{DEBUG_PREFIX} Invalid property name: {token}");
                        }
                        else
                        {
                            Debug.Log($"{DEBUG_PREFIX} Chosen property: {token}");
                            _propertyName = token;
                            _valueParser = CreateValueParser(token);
                        }
                        _searchForProperty = false;
                    }
                    else if (_valueParser != null)
                    {
                        _valueParser.ProcessToken(token);
                        if (_valueParser.Finished)
                        {
                            ApplyChange(_valueParser.Value);
                            _valueParser = null;
                        }
                    }
                }
            }
            private bool IsValidProperty(string propertyName)
            {
                return LEVEL_PROPERTIES.Contains(propertyName);
            }
            private IValueParser CreateValueParser(string propertyName)
            {
                switch (propertyName)
                {
                    case "COLOR_ID":
                        return new SingleValueParser<string>();
                    case "IS_PLAYER":
                        return new SingleValueParser<bool>();
                    case "IS_FLIPPED":
                        return new SingleValueParser<bool>();
                    case "IS_POSSESSABLE":
                        return new SingleValueParser<bool>();
                    case "IS_SECONDARY_PLAYER":
                        return new SingleValueParser<bool>();
                    case "CUBE_GRID":
                        return new Array2DParser<string>();
                }
                return null;
            }
            private void ApplyChange(object value)
            {
                if (_cubeDetails == null)
                {
                    Debug.Log($"{DEBUG_PREFIX} Cannot modify property when cube details is null");
                    return;
                }
                switch (_propertyName)
                {
                    case "COLOR_ID":
                        if (value is string colorID)
                        {
                            _cubeDetails.ColorID = colorID;
                        }
                        else
                        {
                            Debug.Log($"Receive invalid value type for {_propertyName}");
                        }
                        break;
                    case "IS_PLAYER":
                        if (value is bool isPlayer)
                        {
                            _cubeDetails.IsPlayer = isPlayer;
                        }
                        else
                        {
                            Debug.Log($"Receive invalid value type for {_propertyName}");
                        }
                        break;
                    case "IS_FLIPPED":
                        if (value is bool isFlipped)
                        {
                            _cubeDetails.IsFlipped = isFlipped;
                        }
                        else
                        {
                            Debug.Log($"Receive invalid value type for {_propertyName}");
                        }
                        break;
                    case "IS_POSSESSABLE":
                        if (value is bool isPossessable)
                        {
                            _cubeDetails.IsPossessable = isPossessable;
                        }
                        else
                        {
                            Debug.Log($"Receive invalid value type for {_propertyName}");
                        }
                        break;
                    case "IS_SECONDARY_PLAYER":
                        if (value is bool isSecondaryPlayer)
                        {
                            _cubeDetails.IsSecondaryPlayer = isSecondaryPlayer;
                        }
                        else
                        {
                            Debug.Log($"Receive invalid value type for {_propertyName}");
                        }
                        break;
                    case "CUBE_GRID":
                        if (value is List<List<string>> cubeGrid)
                        {
                            _cubeDetails.CubeGrid = cubeGrid;
                        }
                        else
                        {
                            Debug.Log($"Receive invalid value type for {_propertyName}");
                        }
                        break;
                }
            }
        }
    }

    private interface IValueParser
    {
        public object Value { get; }
        public bool Finished { get; }
        public void ProcessToken(string token);
    }

    private class SingleValueParser<T> : IValueParser
    {
        private object _value = null;
        private bool _finished = false;

        public object Value
        {
            get => _value;
        }
        public bool Finished
        {
            get => _finished;
        }

        public void ProcessToken(string token)
        {
            if (_finished)
            {
                return;
            }

            _value = Convert.ChangeType(token, typeof(T));
            _finished = true;
        }
    }
    private class ArrayParser<T> : IValueParser
    {
        private const string DEBUG_PREFIX = "[ArrayParser]";

        private const string ARRAY_START_SYMBOL = "[";
        private const string ARRAY_END_SYMBOL = "]";

        private object _value = null;
        private bool _finished = false;

        private List<T> _result = new();
        private bool _isStoring = false;

        public object Value
        {
            get => _value;
        }

        public bool Finished
        {
            get => _finished;
        }

        public void ProcessToken(string token)
        {
            if (_finished)
            {
                return;
            }

            Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (!_isStoring)
            {
                if (token == ARRAY_START_SYMBOL)
                {
                    Debug.Log($"{DEBUG_PREFIX} Start storing!");
                    _isStoring = true;
                }
            }
            else
            {
                if (token == ARRAY_END_SYMBOL)
                {
                    Debug.Log($"{DEBUG_PREFIX} Stop storing!");
                    _finished = true;
                    _value = _result;
                    _isStoring = false;
                }
                else
                {
                    T newValue = (T)Convert.ChangeType(token, typeof(T));
                    if (newValue != null)
                    {
                        _result.Add(newValue);
                    }
                }
            }
        }
    }

    private class Array2DParser<T> : IValueParser
    {
        private const string DEBUG_PREFIX = "[Array2DParser]";

        private const string ARRAY_START_SYMBOL = "[";
        private const string ARRAY_END_SYMBOL = "]";

        private object _value = null;
        private bool _finished = false;

        private List<List<T>> _result;
        private bool _isStoring = false;
        private int _arrayStartSymbolCount = 0;
        private ArrayParser<T> _arrayParser = null;

        public object Value
        {
            get => _value;
        }

        public bool Finished
        {
            get => _finished;
        }

        public void ProcessToken(string token)
        {
            if (_finished)
            {
                return;
            }

            Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (!_isStoring)
            {
                if (token == ARRAY_START_SYMBOL)
                {
                    _arrayStartSymbolCount++;
                    _isStoring = true;
                }
            }
            else
            {
                if (token == ARRAY_START_SYMBOL)
                {
                    _arrayStartSymbolCount++;
                }

                if (token == ARRAY_END_SYMBOL)
                {
                    if (_arrayStartSymbolCount <= 1)
                    {
                        _finished = true;
                        _isStoring = false;
                        _value = _result;
                        return;
                    }

                    _arrayStartSymbolCount--;
                }

                _arrayParser ??= new();
                _arrayParser.ProcessToken(token);
                if (_arrayParser.Finished)
                {
                    if (_arrayParser.Value is List<T> value)
                    {
                        _result ??= new();
                        _result.Add(value);
                    }
                    _arrayParser = null;
                }
            }
        }
    }
}
