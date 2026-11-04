namespace MKS.Shared.Domain.Shared.Constants
{
    /// <summary>Defines stable integration-event validation messages.</summary>
    public static class IntegrationEventConstants
    {
        /// <summary>Used when app and tenant identifiers conflict with the event scope.</summary>
        public const string InvalidScopeIdentifiers =
            "Integration event app and tenant identifiers do not match a valid isolation scope.";
    }
}
