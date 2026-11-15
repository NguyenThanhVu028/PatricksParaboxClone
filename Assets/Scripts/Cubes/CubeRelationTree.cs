using System;
using System.Collections.Generic;
using UnityEngine;

// Data that holds all the relations between cubes
public class CubeRelationTree
{
    private const string DEBUG_PREFIX = "[CubeRelationTree]";

    private Dictionary<BaseCube, Relation> _relations;

    public Relation GetRelation(BaseCube cube)
    {
        if (_relations.TryGetValue(cube, out var relation))
        {
            return relation;
        }
        return default;
    }

    public void EstablishRelation(BaseCube parentCube, BaseCube childCube)
    {
        if (childCube == null)
        {
            Debug.LogWarning($"{DEBUG_PREFIX} Cannot establish relation on a null child cube!");
            return;
        }


        // Make sure both child cube and parent cube exist in the tree first!
        if (!_relations.ContainsKey(childCube))
        {
            _relations.Add(childCube, new Relation(childCube, parentCube));
        }

        if (parentCube != null)
        {
            if (!_relations.ContainsKey(parentCube))
            {
                _relations.Add(parentCube, new(parentCube, null));
            }
        }

        // Remove child cube from its current parent
        var childCubeRelation = _relations[childCube];
        if (childCubeRelation.ParentCube == parentCube)
        {
            return;
        }
        if (_relations.TryGetValue(childCubeRelation.ParentCube, out Relation currentParentRelation))
        {
            currentParentRelation.SelfGrid.RemoveChild(childCube);
        }

        // Establish new relation with the new parent
        childCubeRelation.ParentCube = parentCube;
    }

    public BaseCube GetParent(BaseCube childCube)
    {
        if (childCube == null || !_relations.TryGetValue(childCube, out Relation relation))
        {
            return null;
        }

        return relation.ParentCube;
    }

    public List<BaseCube> GetChildren(BaseCube parentCube, Func<BaseCube, bool> condition = null)
    {
        var result = new List<BaseCube>();
        if (parentCube == null || !_relations.TryGetValue(parentCube, out Relation relation))
        {
            return result;
        }

        return relation.SelfGrid?.GetChildren(condition);
    }

}

public class Relation
{
    public BaseCube ParentCube;
    public CubeGrid SelfGrid;

    public Relation(BaseCube selfCube, BaseCube parentCube = null)
    {
        ParentCube = parentCube;
        SelfGrid = selfCube.GetComponent<CubeGrid>();
    }
}
