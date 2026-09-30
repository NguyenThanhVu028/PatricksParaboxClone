using System.Collections.Generic;
using UnityEngine;

// Data that holds all the relations between cubes
public class CubeRelations
{
    private Dictionary<Cube, Relation> _relations;

    public IReadOnlyDictionary<Cube, Relation> Relations
    {
        get => _relations;
    }

    public Relation GetRelation(Cube cube)
    {
        if (_relations.TryGetValue(cube, out var relation))
        {
            return relation;
        }
        return default;
    }

    /// <summary>
    /// Establishes a parent-child relationship between the specified child and parent cubes.
    /// </summary>
    /// <param name="childCube">The child cube to establish the relationship for.</param>
    /// <param name="parentCube">The parent cube to establish the relationship with.</param>
    /// <returns>True if the relationship was successfully established; otherwise, false.</returns>
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
                _relations[childRelation.ParentCube] = oldParentRelation;
            }
        }

        // Establish the new parent-child relationship.
        if (parentCube != null)
        {
            var parentRelation = _relations[parentCube];
            parentRelation.ChildCubes?.Add(childCube);
            _relations[parentCube] = parentRelation;
        }

        childRelation.ParentCube = parentCube;
        _relations[childCube] = childRelation;
    }
}

public struct Relation
{
    public Cube ParentCube;
    public List<Cube> ChildCubes;

    public Relation(Cube parentCube = null, List<Cube> childCubes = null)
    {
        ParentCube = parentCube;
        ChildCubes = childCubes ?? new List<Cube>();
    }
}
