namespace eShop.Ordering.API.Application.Queries;

public class OrderQueries(OrderingContext context)
    : IOrderQueries
{
    public async Task<Order> GetOrderAsync(int id)
    {
        var orders = await context.Orders.ToListAsync();
        var order = orders.FirstOrDefault(o => o.Id == id);
      
        if (order is null)
            throw new KeyNotFoundException();

        await context.Entry(order).Reference(o => o.Address).LoadAsync();
        var orderItems = await context.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();

        return new Order
        {
            OrderNumber = order.Id,
            Date = order.OrderDate,
            Description = order.Description,
            City = order.Address?.City ?? "",
            Country = order.Address?.Country ?? "",
            State = order.Address?.State ?? "",
            Street = order.Address?.Street ?? "",
            Zipcode = order.Address?.ZipCode ?? "",
            Status = order.OrderStatus.ToString(),
            Total = orderItems.Sum(oi => (double)(oi.UnitPrice * oi.Units)),
            OrderItems = orderItems.Select(oi => new Orderitem
            {
                ProductName = oi.ProductName,
                Units = oi.Units,
                UnitPrice = (double)oi.UnitPrice,
                PictureUrl = oi.PictureUrl
            }).ToList()
        };
    }

    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId)
    {
        var orders = await context.Orders
            .Where(o => o.Buyer.IdentityGuid == userId)  
            .ToListAsync();
        
        var summaries = new List<OrderSummary>();
        foreach (var order in orders)
        {
            var orderItems = await context.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
            summaries.Add(new OrderSummary
            {
                OrderNumber = order.Id,
                Date = order.OrderDate,
                Status = order.OrderStatus.ToString(),
                Total = (double)orderItems.Sum(oi => oi.UnitPrice * oi.Units)
            });
        }
        return summaries;
    } 
    
    public async Task<IEnumerable<CardType>> GetCardTypesAsync() => 
        await context.CardTypes.Select(c=> new CardType { Id = c.Id, Name = c.Name }).ToListAsync();
}
