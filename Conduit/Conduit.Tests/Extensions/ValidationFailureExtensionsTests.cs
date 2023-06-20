using Conduit.Web.Extensions;
using FluentValidation.Results;

namespace Conduit.Tests.Extensions;

[TestFixture]
public class ValidationFailureExtensionsTests
{
    private List<ValidationFailure> _failures;

    [SetUp]
    public async Task Setup()
    {
        _failures = new List<ValidationFailure>();
    }

    [Test]
    public async Task MultipleErrors_are_concatenated_as_csv()
    {
        // arrange
        _failures.Add(new ValidationFailure("foo", "error1"));
        _failures.Add(new ValidationFailure("bar", "error2"));

        // act
        var result = _failures.ToCsv();

        // assert
        Assert.That(result, Is.EqualTo("error1,error2"));
    }

    [Test]
    public async Task Null_collection_returns_empty_string()
    {
        // arrange
        _failures = null;

        // act
        var result = _failures.ToCsv();

        // assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task Empty_collection_returns_empty_string()
    {
        // no arrange needed

        // act
        var result = _failures.ToCsv();

        // assert
        Assert.That(result, Is.Empty);
    }
}