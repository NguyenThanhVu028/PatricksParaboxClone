using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using System.Linq;

public class LevelDesignCompiler
{
    private const string DEBUG_PREFIX = "[LevelDetailsConverter]";
    private const string COMMAND_SYMBOL = "#";
    private const string COMMENT_SYMBOL = "//";
    private const string ARRAY_START_SYMBOL = "[";
    private const string ARRAY_END_SYMBOL = "]";

    private static bool _debugProcess = false;
    private static bool _debugWarnings = true;

    public static LevelDetails CompileToLevelDetails(string filePath)
    {
        ICommand currentCommand = null;
        bool isSearchForCommand = false;
        bool isCommenting = false;

        if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Begin compiling file at {filePath}");

        if (string.IsNullOrEmpty(filePath))
        {
            if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} File path is null or empty.");
            return null;
        }

        if (!File.Exists(filePath))
        {
            if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} File at {filePath} does not exist");
            return null;
        }

        string fileContent = File.ReadAllText(filePath);
        var tokens = StringUtils.ParseString(fileContent);
        if (tokens == null)
        {
            if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} An error occurred, cannot parse into tokens!");
            return null;
        }

        if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Successfully extracted {tokens.Count} tokens");
        LevelDetails levelDetails = new();

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] == null)
            {
                continue;
            }

            if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Processing token: {tokens[i]}");

            if (tokens[i] == COMMENT_SYMBOL)
            {
                if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} {(isCommenting ? "Stop" : "Start")} commenting");
                isCommenting = !isCommenting;
                continue;
            }

            // Skip all tokens when commenting
            if (isCommenting)
            {
                continue;
            }

            if (tokens[i] == GetCommandSymbolByLevel(1))
            {
                if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Command symbol detected");
                currentCommand = null;
                isSearchForCommand = true;
            }
            else
            {
                if (isSearchForCommand)
                {
                    // Only search for command name in the very first token that follows the command symbol
                    // If no valid command name is found -> Discard the command
                    currentCommand = CreateCommand(tokens[i], levelDetails);
                    if (currentCommand == null)
                    {
                        if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Invalid command: {tokens[i]}");
                    }
                    else
                    {
                        if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Find new command: {tokens[i]}");
                    }
                    isSearchForCommand = false;
                }
                else
                {
                    currentCommand?.ProcessToken(tokens[i]);
                }
            }
        }

        return levelDetails;
    }

    private static ICommand CreateCommand(string command, LevelDetails levelDetails)
    {
        ICommand newCommand = null;
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
    public static string GetCommandSymbolByLevel(int level)
    {
        string result = "";
        for (int i = 1; i <= level; i++)
        {
            result += COMMAND_SYMBOL;
        }
        return result;
    }

    private interface ICommand
    {
        public void Init(LevelDetails levelDetails);
        public void ProcessToken(string token);
    }
    private class LevelConfigCommand : ICommand
    {
        private enum LevelProperties
        {
            TIME
        }

        private const string DEBUG_PREFIX = "[LevelConfigCommand]";

        private LevelDetails _levelDetails = null;
        private IValueParser _currentValueParser = null;
        private bool _isSearchForProperty = false;
        private LevelProperties _currentProperty;

        public void Init(LevelDetails levelDetails)
        {
            _levelDetails = levelDetails;
        }
        public void ProcessToken(string token)
        {
            if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (token == null)
            {
                if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Invalid token!");
                return;
            }

            if (_levelDetails == null)
            {
                if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Level details needs to be init!");
                return;
            }

            if (token == GetCommandSymbolByLevel(2))
            {
                if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Property command detected");
                _currentValueParser = null;
                _isSearchForProperty = true;
            }
            else
            {
                if (_isSearchForProperty)
                {
                    if (!IsValidProperty(token))
                    {
                        if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Invalid property name: {token}");
                    }
                    else
                    {
                        if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Chosen property: {token}");
                        if (Enum.TryParse(token, out _currentProperty))
                        {
                            _currentValueParser = CreateValueParser(_currentProperty);
                        }
                    }
                    _isSearchForProperty = false;
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
            return Enum.TryParse(propertyName, out LevelProperties property) && Enum.IsDefined(typeof(LevelProperties), property);
        }
        private IValueParser CreateValueParser(LevelProperties property)
        {
            return property switch
            {
                LevelProperties.TIME => new SingleValueParser<float>(),
                _ => null,
            };
        }
        private void ApplyChange(object value)
        {
            if (_levelDetails == null)
            {
                if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Cannot modify property when level details is null");
                return;
            }
            switch (_currentProperty)
            {
                case LevelProperties.TIME:
                    if (value is float floatValue)
                    {
                        _levelDetails.Time = floatValue;
                        if (_debugProcess) Debug.Log($"Apply new value: {floatValue} for {_currentProperty}");
                    }
                    else
                    {
                        if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                    }
                    break;
            }
        }
    }
    private class CubesCommand : ICommand
    {
        private const string DEBUG_PREFIX = "[CubesCommand]";

        private LevelDetails _levelDetails = null;
        private bool _isSearchForCubeID = false;
        private CubeDetailsCommand _currentCubeDetailsCommand = null;

        public void Init(LevelDetails levelDetails)
        {
            _levelDetails = levelDetails;
        }

        public void ProcessToken(string token)
        {
            if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (token == null)
            {
                if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Invalid token!");
                return;
            }
            if (_levelDetails == null)
            {
                if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Level details needs to be init!");
                return;
            }

            if (token == GetCommandSymbolByLevel(2))
            {
                if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Cube details command detected");
                _isSearchForCubeID = true;
                _currentCubeDetailsCommand = null;
            }
            else
            {
                if (_isSearchForCubeID)
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
                    _isSearchForCubeID = false;
                    if (_debugProcess) Debug.Log($"Begin modifying cube details with id: {cubeID}");
                }
                else
                {
                    _currentCubeDetailsCommand?.ProcessToken(token);
                }
            }
        }
        private class CubeDetailsCommand : ICommand
        {
            private enum CubeProperties
            {
                CUBE_TYPE,
                COLOR_ID,
                IS_PLAYER,
                IS_FLIPPED,
                IS_POSSESSABLE,
                IS_SECONDARY_PLAYER,
                MAIN_CUBE_ID,
                CUBE_GRID,
            }
            private const string DEBUG_PREFIX = "[CubeDetailsCommand]";

            private CubeDetails _cubeDetails = null;
            private IValueParser _valueParser = null;
            private bool _searchForProperty = false;
            private CubeProperties _currentProperty;
            public CubeDetailsCommand(CubeDetails targetCubeDetails)
            {
                _cubeDetails = targetCubeDetails;
            }

            public void Init(LevelDetails levelDetails) { }

            public void ProcessToken(string token)
            {
                if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

                if (token == null)
                {
                    if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Invalid token!");
                    return;
                }
                if (_cubeDetails == null)
                {
                    if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Cube details needs to be init!");
                    return;
                }

                if (token == GetCommandSymbolByLevel(3))
                {
                    if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Property command detected");
                    _valueParser = null;
                    _searchForProperty = true;
                }
                else
                {
                    if (_searchForProperty)
                    {
                        if (!IsValidProperty(token))
                        {
                            if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Invalid property name: {token}");
                        }
                        else
                        {
                            if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Chosen property: {token}");
                            if (Enum.TryParse(token, out _currentProperty))
                            {
                                _valueParser = CreateValueParser(_currentProperty);
                            }
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
                return Enum.TryParse(propertyName, out CubeProperties property) && Enum.IsDefined(typeof(CubeProperties), property);
            }
            private IValueParser CreateValueParser(CubeProperties property)
            {
                return property switch
                {
                    CubeProperties.CUBE_TYPE => new SingleValueParser<CubeTypes>(),
                    CubeProperties.COLOR_ID => new SingleValueParser<string>(),
                    CubeProperties.IS_PLAYER => new SingleValueParser<bool>(),
                    CubeProperties.IS_FLIPPED => new SingleValueParser<bool>(),
                    CubeProperties.IS_POSSESSABLE => new SingleValueParser<bool>(),
                    CubeProperties.IS_SECONDARY_PLAYER => new SingleValueParser<bool>(),
                    CubeProperties.MAIN_CUBE_ID => new SingleValueParser<string>(),
                    CubeProperties.CUBE_GRID => new Array2DParser<string>(),
                    _ => null,
                };
            }
            private void ApplyChange(object value)
            {
                if (_cubeDetails == null)
                {
                    if (_debugWarnings) Debug.LogWarning($"{DEBUG_PREFIX} Cannot modify property when cube details is null");
                    return;
                }
                switch (_currentProperty)
                {
                    case CubeProperties.CUBE_TYPE:
                        if (value is CubeTypes cubeType)
                        {
                            _cubeDetails.CubeType = cubeType;
                            if (_debugProcess) Debug.Log($"Apply new value: {cubeType} to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                        }
                        break;
                    case CubeProperties.COLOR_ID:
                        if (value is string colorID)
                        {
                            _cubeDetails.ColorID = colorID;
                            if (_debugProcess) Debug.Log($"Apply new value: {colorID} to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                        }
                        break;
                    case CubeProperties.IS_PLAYER:
                        if (value is bool isPlayer)
                        {
                            _cubeDetails.IsPlayer = isPlayer;
                            if (_debugProcess) Debug.Log($"Apply new value: {isPlayer} to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                        }
                        break;
                    case CubeProperties.IS_FLIPPED:
                        if (value is bool isFlipped)
                        {
                            _cubeDetails.IsFlipped = isFlipped;
                            if (_debugProcess) Debug.Log($"Apply new value: {isFlipped} to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                        }
                        break;
                    case CubeProperties.IS_POSSESSABLE:
                        if (value is bool isPossessable)
                        {
                            _cubeDetails.IsPossessable = isPossessable;
                            if (_debugProcess) Debug.Log($"Apply new value: {isPossessable} to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                        }
                        break;
                    case CubeProperties.IS_SECONDARY_PLAYER:
                        if (value is bool isSecondaryPlayer)
                        {
                            _cubeDetails.IsSecondaryPlayer = isSecondaryPlayer;
                            if (_debugProcess) Debug.Log($"Apply new value: {isSecondaryPlayer} to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                        }
                        break;
                    case CubeProperties.MAIN_CUBE_ID:
                        if (value is string mainCubeID)
                        {
                            _cubeDetails.MainCubeID = mainCubeID;
                            if (_debugProcess) Debug.Log($"Apply new value: {mainCubeID} to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
                        }
                        break;
                    case CubeProperties.CUBE_GRID:
                        if (value is List<List<string>> cubeGrid)
                        {
                            _cubeDetails.CubeGrid = cubeGrid;
                            if (_debugProcess) Debug.Log($"Apply new value to {_currentProperty}");
                        }
                        else
                        {
                            if (_debugWarnings) Debug.LogWarning($"Receive invalid value type for {_currentProperty}");
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

            if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");
            if (typeof(T).IsEnum)
            {
                Enum.TryParse(typeof(T), token, out _value);
            }
            else
            {
                _value = (T)Convert.ChangeType(token, typeof(T));
            }
            _finished = true;
        }
    }
    private class ArrayParser<T> : IValueParser
    {
        private const string DEBUG_PREFIX = "[ArrayParser]";

        private List<T> _value = new();
        private bool _finished = false;

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

            if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (!_isStoring)
            {
                if (token == ARRAY_START_SYMBOL)
                {
                    if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Start storing!");
                    _isStoring = true;
                }
            }
            else
            {
                if (token == ARRAY_END_SYMBOL)
                {
                    if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Stop storing!");
                    _finished = true;
                    _isStoring = false;
                }
                else
                {
                    T newValue = (T)Convert.ChangeType(token, typeof(T));
                    if (newValue != null)
                    {
                        _value ??= new();
                        _value.Add(newValue);
                    }
                }
            }
        }
    }
    private class Array2DParser<T> : IValueParser
    {
        private const string DEBUG_PREFIX = "[Array2DParser]";


        private List<List<T>> _value = null;
        private bool _finished = false;

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

            if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Processing token: {token}");

            if (!_isStoring)
            {
                if (token == ARRAY_START_SYMBOL)
                {
                    if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Start symbol detected");
                    _arrayStartSymbolCount++;
                    _isStoring = true;
                }
            }
            else
            {
                if (token == ARRAY_START_SYMBOL)
                {
                    if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} Start symbol detected");
                    _arrayStartSymbolCount++;
                }

                if (token == ARRAY_END_SYMBOL)
                {
                    if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} End symbol detected");
                    if (_arrayStartSymbolCount <= 1)
                    {
                        _finished = true;
                        _isStoring = false;
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
                        _value ??= new();
                        _value.Add(value);
                        if (_debugProcess) Debug.Log($"{DEBUG_PREFIX} New row added");
                    }
                    _arrayParser = null;
                }
            }
        }
    }
}
