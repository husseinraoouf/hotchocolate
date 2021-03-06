using Microsoft.Extensions.DependencyInjection;
using HotChocolate.Language;
using Xunit;

namespace HotChocolate.Validation
{
    public class VariableUniquenessRuleTests
        : DocumentValidatorVisitorTestBase
    {
        public VariableUniquenessRuleTests()
            : base(services => services.AddVariableUniqueAndInputTypeRule())
        {
        }

        [Fact]
        public void OperationWithTwoVariablesThatHaveTheSameName()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                query houseTrainedQuery($atOtherHomes: Boolean, $atOtherHomes: Boolean) {
                    dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                    }
                }
            ");
            context.Prepare(query);

            // act
            Rule.Validate(context, query);

            // assert
            Assert.Single(context.Errors);
            Assert.Collection(context.Errors,
                t => Assert.Equal(
                    "A document containing operations that " +
                    "define more than one variable with the same " +
                    "name is invalid for execution.", t.Message));
        }

        [Fact]
        public void NoOperationHasVariablesThatShareTheSameName()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                query ($foo: Boolean = true, $bar: Boolean = false) {
                    field @skip(if: $foo) {
                        subfieldA
                    }
                    field @skip(if: $bar) {
                        subfieldB
                    }
                }
            ");
            context.Prepare(query);

            // act
            Rule.Validate(context, query);

            // assert
            Assert.Empty(context.Errors);
        }

        [Fact]
        public void TwoOperationsThatShareVariableName()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                query A($atOtherHomes: Boolean) {
                  ...HouseTrainedFragment
                }

                query B($atOtherHomes: Boolean) {
                  ...HouseTrainedFragment
                }

                fragment HouseTrainedFragment on Query {
                  dog {
                    isHousetrained(atOtherHomes: $atOtherHomes)
                  }
                }
            ");
            context.Prepare(query);

            // act
            Rule.Validate(context, query);

            // assert
            Assert.Empty(context.Errors);
        }
    }
}
