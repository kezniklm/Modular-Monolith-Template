using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine.Http;

namespace Module.Presentation.Endpoints;

public static class CreateItemsEndpoint
{
    [WolverinePost("/items")]
    [Tags("module")]
    [EndpointSummary("Create a new item")]
    [EndpointDescription("Creates a new item with the specified name and price")]
    public static Created<ItemCreatedResponse> Create(CreateItemRequest request)
    {
        var newId = 1;
        var response = new ItemCreatedResponse(newId, request.Name, request.Price);

        return TypedResults.Created($"/items/{newId}", response);
    }

    public record CreateItemRequest(string Name, decimal Price);

    public record ItemCreatedResponse(int Id, string Name, decimal Price);
}
