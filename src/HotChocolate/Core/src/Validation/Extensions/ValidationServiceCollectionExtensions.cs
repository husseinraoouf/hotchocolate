using HotChocolate.Validation;
using HotChocolate.Validation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HotChocolateValidationServiceCollectionExtensions
    {
        public static IServiceCollection AddValidation(
            this IServiceCollection services)
        {
            services.TryAddSingleton(sp => new DocumentValidatorContextPool(8));
            services.TryAddSingleton<IDocumentValidator, DocumentValidator>();

            services
                .AddDirectivesAreValidRule()
                .AddExecutableDefinitionsRule()
                .AddFieldMustBeDefinedRule()
                .AddFragmentsAreValidRule()
                .AddAllVariablesUsedRule()
                .AddAllVariableUsagesAreAllowedRule()
                .AddVariableUniqueAndInputTypeRule();

            return services;
        }

        /// <summary>
        /// GraphQL servers define what directives they support.
        /// For each usage of a directive, the directive must be available
        /// on that server.
        ///
        /// http://spec.graphql.org/June2018/#sec-Directives-Are-Defined
        ///
        /// AND
        ///
        /// GraphQL servers define what directives they support and where they
        /// support them.
        ///
        /// For each usage of a directive, the directive must be used in a
        /// location that the server has declared support for.
        ///
        /// http://spec.graphql.org/June2018/#sec-Directives-Are-In-Valid-Locations
        ///
        /// AND
        ///
        /// Directives are used to describe some metadata or behavioral change on
        /// the definition they apply to.
        ///
        /// When more than one directive of the
        /// same name is used, the expected metadata or behavior becomes ambiguous,
        /// therefore only one of each directive is allowed per location.
        ///
        /// http://spec.graphql.org/draft/#sec-Directives-Are-Unique-Per-Location
        /// </summary>
        public static IServiceCollection AddDirectivesAreValidRule(
            this IServiceCollection services)
        {
            return services.AddValidationRule<DirectivesVisitor>();
        }

        /// <summary>
        /// GraphQL execution will only consider the executable definitions
        /// Operation and Fragment.
        ///
        /// Type system definitions and extensions are not executable,
        /// and are not considered during execution.
        ///
        /// To avoid ambiguity, a document containing TypeSystemDefinition
        /// is invalid for execution.
        ///
        /// GraphQL documents not intended to be directly executed may
        /// include TypeSystemDefinition.
        ///
        /// http://spec.graphql.org/June2018/#sec-Executable-Definitions
        /// </summary>
        public static IServiceCollection AddExecutableDefinitionsRule(
            this IServiceCollection services)
        {
            return services.AddSingleton<IDocumentValidatorRule, ExecutableDefinitionsRule>();
        }

        /// <summary>
        /// The target field of a field selection must be defined on the scoped
        /// type of the selection set. There are no limitations on alias names.
        ///
        /// http://spec.graphql.org/June2018/#sec-Field-Selections-on-Objects-Interfaces-and-Unions-Types
        /// </summary>
        public static IServiceCollection AddFieldMustBeDefinedRule(
            this IServiceCollection services)
        {
            return services.AddValidationRule<FieldMustBeDefinedVisitor>();
        }

        /// <summary>
        /// Fragment definitions are referenced in fragment spreads by name.
        /// To avoid ambiguity, each fragment’s name must be unique within a
        /// document.
        ///
        /// http://spec.graphql.org/June2018/#sec-Fragment-Name-Uniqueness
        ///
        /// AND
        ///
        /// Defined fragments must be used within a document.
        ///
        /// http://spec.graphql.org/June2018/#sec-Fragments-Must-Be-Used
        ///
        /// AND
        ///
        /// Fragments can only be declared on unions, interfaces, and objects.
        /// They are invalid on scalars.
        /// They can only be applied on non‐leaf fields.
        /// This rule applies to both inline and named fragments.
        ///
        /// http://spec.graphql.org/June2018/#sec-Fragments-On-Composite-Types
        ///
        /// AND
        ///
        /// Fragments are declared on a type and will only apply when the
        /// runtime object type matches the type condition.
        ///
        /// They also are spread within the context of a parent type.
        ///
        /// A fragment spread is only valid if its type condition could ever
        /// apply within the parent type.
        ///
        /// http://spec.graphql.org/June2018/#sec-Fragment-spread-is-possible
        ///
        /// AND
        ///
        /// Named fragment spreads must refer to fragments defined within the
        /// document.
        ///
        /// It is a validation error if the target of a spread is not defined.
        ///
        /// http://spec.graphql.org/June2018/#sec-Fragment-spread-target-defined
        ///
        /// AND
        ///
        /// The graph of fragment spreads must not form any cycles including
        /// spreading itself. Otherwise an operation could infinitely spread or
        /// infinitely execute on cycles in the underlying data.
        ///
        /// http://spec.graphql.org/June2018/#sec-Fragment-spreads-must-not-form-cycles
        ///
        /// AND
        ///
        /// Fragments must be specified on types that exist in the schema.
        /// This applies for both named and inline fragments.
        /// If they are not defined in the schema, the query does not validate.
        ///
        /// http://spec.graphql.org/June2018/#sec-Fragment-Spread-Type-Existence
        /// </summary>
        public static IServiceCollection AddFragmentsAreValidRule(
            this IServiceCollection services)
        {
            return services.AddValidationRule<FragmentsVisitor>();
        }

        /// <summary>
        /// All variables defined by an operation must be used in that operation
        /// or a fragment transitively included by that operation.
        ///
        /// Unused variables cause a validation error.
        ///
        /// http://spec.graphql.org/June2018/#sec-All-Variables-Used
        ///
        /// AND
        ///
        /// Variables are scoped on a per‐operation basis. That means that
        /// any variable used within the context of an operation must be defined
        /// at the top level of that operation
        ///
        /// http://spec.graphql.org/June2018/#sec-All-Variable-Uses-Defined
        /// </summary>
        public static IServiceCollection AddAllVariablesUsedRule(
            this IServiceCollection services)
        {
            return services.AddValidationRule<AllVariablesUsedVisitor>();
        }

        /// <summary>
        /// Variable usages must be compatible with the arguments
        /// they are passed to.
        ///
        /// Validation failures occur when variables are used in the context
        /// of types that are complete mismatches, or if a nullable type in a
        ///  variable is passed to a non‐null argument type.
        ///
        /// http://spec.graphql.org/June2018/#sec-All-Variable-Usages-are-Allowed
        /// </summary>
        public static IServiceCollection AddAllVariableUsagesAreAllowedRule(
            this IServiceCollection services)
        {
            return services.AddValidationRule<AllVariableUsagesAreAllowedVisitor>();
        }

        /// <summary>
        /// If any operation defines more than one variable with the same name,
        /// it is ambiguous and invalid. It is invalid even if the type of the
        /// duplicate variable is the same.
        ///
        /// http://spec.graphql.org/June2018/#sec-Validation.Variables
        ///
        /// AND
        ///
        /// Variables can only be input types. Objects,
        /// unions, and interfaces cannot be used as inputs.
        ///
        /// http://spec.graphql.org/June2018/#sec-Variables-Are-Input-Types
        /// </summary>
        public static IServiceCollection AddVariableUniqueAndInputTypeRule(
            this IServiceCollection services)
        {
            return services.AddValidationRule<VariableUniqueAndInputTypeVisitor>();
        }

        public static IServiceCollection AddValidationRule<T>(
            this IServiceCollection services)
            where T : DocumentValidatorVisitor, new()
        {
            return services.AddSingleton<IDocumentValidatorRule, DocumentValidatorRule<T>>();
        }
    }
}
