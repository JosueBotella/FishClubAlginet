//using System.Reflection;


//namespace FishClubAlginet.Application.Features.Fishermen;

//internal sealed record GetAllFisherMenQuery();

//internal sealed record FisherManDto(string Id, string Name);

//internal class FisherManGetAllQueriesHandler
//{
//    private readonly object _commandHandler;
    

//    public FisherManGetAllQueriesHandler(object commandHandler)
//    {
//        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
//    }

//    public async Task<IEnumerable<FisherManDto>> HandleAsync(GetAllFisherMenQuery query, CancellationToken cancellationToken = default)
//    {
       

        
        
//        object? awaitedResult;
//        try
//        {
//            // invokeResult is expected to be a Task or Task<T>
//            awaitedResult = await (dynamic)invokeResult;
//        }
//        catch
//        {
//            // If the method returned a non-task result or fail to await, try to use it directly
//            awaitedResult = invokeResult;
//        }

//        if (awaitedResult == null)
//            return Array.Empty<FisherManDto>();

//        // Try to treat awaitedResult as IEnumerable
//        if (awaitedResult is System.Collections.IEnumerable enumerable)
//        {
//            var list = new List<FisherManDto>();
//            foreach (var item in enumerable)
//            {
//                if (item == null) continue;
//                var dto = MapToDto(item);
//                if (dto != null) list.Add(dto);
//            }

//            return list;
//        }

//        // Single item returned: try to map to DTO
//        var singleDto = MapToDto(awaitedResult);
//        return singleDto is null ? Array.Empty<FisherManDto>() : new[] { singleDto };
//    }   
//}
