using System.Collections.Generic;
using UnityEngine;

// Data that holds all the relations between cubes
public class CubeRelationTree
{
    private const string DEBUG_PREFIX = "[CubeRelationTree]";

    private Dictionary<Cube, Relation> _relations;

    public Relation GetRelation(Cube cube)
    {
        if (_relations.TryGetValue(cube, out var relation))
        {
            return relation;
        }
        return default;
    }

    public void EstablishRelation(Cube childCube, Cube parentCube)
    {
        if (childCube == null)
        {
            Debug.LogWarning("Cannot establish relation on a null child cube!");
            return;
        }

        if (!_relations.ContainsKey(childCube))
        {
            _relations.Add(childCube, new Relation());
        }

        if (parentCube != null)
        {
            if (!_relations.ContainsKey(parentCube))
            {
                _relations.Add(parentCube, new Relation());
            }
        }

        var childRelation = _relations[childCube];

        // Remove the child cube from its current parent's child list, if it has a parent.
        if (childRelation.ParentCube != null)
        {
            if (_relations.TryGetValue(childRelation.ParentCube, out var oldParentRelation))
            {
                oldParentRelation.ChildCubes?.Remove(childCube);
            }
        }

        // Establish the new parent-child relationship.
        if (parentCube != null)
        {
            var parentRelation = _relations[parentCube];
            parentRelation.ChildCubes?.Add(childCube);
        }

        childRelation.ParentCube = parentCube;
    }

    public Cube GoUp(Cube targetCube)
    {
        if (targetCube == null)
        {
            return null;
        }

        if (!_relations.ContainsKey(targetCube))
        {
            Debug.Log($"{DEBUG_PREFIX} Cube {targetCube} does not exist in the relation tree!");
            return null;
        }

        return _relations[targetCube].ParentCube;
    }

    public List<Cube> GoDown(Cube targetCube)
    {
        if (targetCube == null)
        {
            return null;
        }

        if (!_relations.ContainsKey(targetCube))
        {
            Debug.Log($"{DEBUG_PREFIX} Cube {targetCube} does not exist in the relation tree!");
            return null;
        }

        return _relations[targetCube].ChildCubes;
    }
}

public class Relation
{
    public Cube ParentCube;
    public List<Cube> ChildCubes;

    public Relation(Cube parentCube = null, List<Cube> childCubes = null)
    {
        ParentCube = parentCube;
        ChildCubes = childCubes ?? new List<Cube>();
    }
}
