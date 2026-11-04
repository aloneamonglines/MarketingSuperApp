namespace MKS.Identity.Application.Contracts.Common.Options
{
    /// <summary>
    /// Configures user creation, sign-in, and break-glass workflow limits.
    /// </summary>
    public sealed class IdentityWorkflowOptions
    {
        /// <summary>Gets or sets the required default password used for newly provisioned accounts.</summary>
        public string DefaultPassword { get; set; } = string.Empty;
        /// <summary>Gets or sets the number of failed sign-ins allowed before temporary lockout.</summary>
        public int FailedSignInThreshold { get; set; } = 5;
        /// <summary>Gets or sets the temporary sign-in lockout duration.</summary>
        public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
        /// <summary>Gets or sets the maximum break-glass duration.</summary>
        public TimeSpan BreakGlassMaximumDuration { get; set; } = TimeSpan.FromMinutes(60);
    }
}
