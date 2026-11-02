
using Core.Domain;

namespace Core.Reports;

public sealed record OrderReportLine(string OrderId, string CustomerId, decimal Total, OrderStatus Status);
