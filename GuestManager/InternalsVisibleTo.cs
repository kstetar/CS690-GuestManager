using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GuestManager.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // Required for Moq to mock internal interfaces