﻿using Microsoft.Extensions.DependencyInjection;
using HotChocolate.Language;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.Validation
{
    public class FragmentsOnCompositeTypesRuleTests
        : DocumentValidatorVisitorTestBase
    {
        public FragmentsOnCompositeTypesRuleTests()
            : base(services => services.AddFragmentsAreValidRule())
        {
        }

        [Fact]
        public void FragOnObject()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                {
                    dog {
                       ... fragOnObject
                    }
                }

                fragment fragOnObject on Dog {
                    name
                }
            ");
            context.Prepare(query);

            // act
            Rule.Validate(context, query);

            // assert
            Assert.Empty(context.Errors);
        }

        [Fact]
        public void FragOnInterface()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                {
                    dog {
                       ... fragOnInterface
                    }
                }

                fragment fragOnInterface on Pet {
                    name
                }
            ");
            context.Prepare(query);

            // act
            Rule.Validate(context, query);

            // assert
            Assert.Empty(context.Errors);
        }

        [Fact]
        public void FragOnUnion()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                {
                    dog {
                       ... fragOnUnion
                    }
                }

                fragment fragOnUnion on CatOrDog {
                    ... on Dog {
                        name
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
        public void FragOnScalar()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                {
                    dog {
                       ... fragOnScalar
                    }
                }

                fragment fragOnScalar on Int {
                    something
                }
            ");
            context.Prepare(query);

            // act
            Rule.Validate(context, query);

            // assert
            Assert.Collection(context.Errors,
                t => Assert.Equal(t.Message,
                    "Fragments can only be declared on unions, interfaces, " +
                    "and objects."));
            context.Errors.MatchSnapshot();
        }

        [Fact]
        public void InlineFragOnScalar()
        {
            // arrange
            IDocumentValidatorContext context = ValidationUtils.CreateContext();
            DocumentNode query = Utf8GraphQLParser.Parse(@"
                {
                    dog {
                       ... inlineFragOnScalar
                    }
                }

                fragment inlineFragOnScalar on Dog {
                    ... on Boolean {
                        somethingElse
                    }
                }
            ");
            context.Prepare(query);

            // act
            Rule.Validate(context, query);

            // assert
            Assert.Collection(context.Errors,
                t => Assert.Equal(t.Message,
                    "Fragments can only be declared on unions, interfaces, " +
                    "and objects."));
            context.Errors.MatchSnapshot();
        }
    }
}
