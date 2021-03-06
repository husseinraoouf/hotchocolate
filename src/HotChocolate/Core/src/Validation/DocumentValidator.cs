using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Language;

namespace HotChocolate.Validation
{
    public sealed class DocumentValidator : IDocumentValidator
    {
        private readonly DocumentValidatorContextPool _contextPool;
        private readonly IDocumentValidatorRule[] _rules;

        public DocumentValidator(
            DocumentValidatorContextPool contextPool,
            IEnumerable<IDocumentValidatorRule> rules)
        {
            if (contextPool is null)
            {
                throw new ArgumentNullException(nameof(contextPool));
            }

            if (rules is null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _contextPool = contextPool;
            _rules = rules.ToArray();
        }

        public DocumentValidatorResult Validate(ISchema schema, DocumentNode document)
        {
            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            DocumentValidatorContext context = _contextPool.Get();

            try
            {
                PrepareContext(schema, document, context);

                for (int i = 0; i < _rules.Length; i++)
                {
                    _rules[i].Validate(context, document);
                }

                if (context.Errors.Count > 0)
                {
                    return new DocumentValidatorResult(context.Errors);
                }
                return DocumentValidatorResult.OK;
            }
            finally
            {
                _contextPool.Return(context);
            }
        }

        private void PrepareContext(
            ISchema schema,
            DocumentNode document,
            DocumentValidatorContext context)
        {
            context.Schema = schema;

            for (int i = 0; i < document.Definitions.Count; i++)
            {
                IDefinitionNode definitionNode = document.Definitions[i];
                if (definitionNode.Kind == NodeKind.FragmentDefinition)
                {
                    var fragmentDefinition = (FragmentDefinitionNode)definitionNode;
                    context.Fragments[fragmentDefinition.Name.Value] = fragmentDefinition;
                }
            }
        }
    }
}
