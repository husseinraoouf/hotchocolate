using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Language;

namespace HotChocolate.Stitching
{
    public interface ISchemaInfo
    {
        string Name { get; }
        DocumentNode Schema { get; }
    }

    public interface ITypeInfo
    {
        ITypeDefinitionNode Definition { get; }

        DocumentNode Schema { get; }

        string SchemaName { get; }
    }

    public interface IMergeSchemaContext
    {
        void AddType(ITypeDefinitionNode type);
    }

    public interface ITypeMerger
    {
        void Merge(
            IMergeSchemaContext context,
            IReadOnlyList<ITypeInfo> types);
    }

    public delegate MergeTypeDelegate MergeTypeFactory(MergeTypeDelegate next);

    public delegate void MergeTypeDelegate(IMergeSchemaContext context, IReadOnlyList<ITypeInfo> types);

    public class MergeEnumType
        : ITypeMerger
    {
        private readonly MergeTypeDelegate _next;

        public MergeEnumType(MergeTypeDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public void Merge(
            IMergeSchemaContext context,
            IReadOnlyList<ITypeInfo> types)
        {
            if (types.All(t => t.Definition is EnumTypeDefinitionNode))
            {
                var first = (EnumTypeDefinitionNode)types[0].Definition;
                StringValueNode description = first.Description;
                var values = new HashSet<string>(
                    first.Values.Select(t => t.Name.Value));

                for (int i = 0; i < types.Count; i++)
                {
                    var other = (EnumTypeDefinitionNode)types[i].Definition;
                    if (AreEqual(values, first))
                    {
                        if (description != null && other.Description != null)
                        {
                            description = other.Description;
                        }
                    }
                }
            }
            else
            {
                _next.Invoke(context, types);
            }
        }

        private bool AreEqual(
            ISet<string> left,
            EnumTypeDefinitionNode right)
        {
            if (left.Count == right.Values.Count)
            {
                for (int i = 0; i < right.Values.Count; i++)
                {
                    if (!left.Contains(right.Values[i].Name.Value))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }

}