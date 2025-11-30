# Shortcuts & Commands

1. Reverse engineering data structures
   Scaffold-DbContext "Connection String" Microsoft.EntityFrameworkCore.SqlServer -ContextDir Data -OutputDir Models [(optional) -ContextNamespace NewNamespace.Data] [(optional) -Tables Categories,Customers,Employees,Orders,Products,Shippers,Suppliers] -Force -DataAnnotation
   dotnet ef dbcontext scaffold "Connection String" Microsoft.EntityFrameworkCore.SqlServer -ContextDir Data -OutputDir Models [(optional) -ContextNamespace NewNamespace.Data] [(optional) -Namespace NewNamespace.Models] [(optional) -Tables Categories,Customers,Employees,Orders,Products,Shippers,Suppliers] -Force -DataAnnotation

2. Manage secrets
   1. Right click on project -> Manage User Secrets
   2. Create an entry for the connection string

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Northwind;Trusted_Connection=True;MultipleActiveResultSets=true"
	"DockerConnection": "Data Source=127.0.0.1,1434;Initial Edm=zeh;User ID=sa;Password=A!234567a;TrustServerCertificate=True;"
  }
}
```

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=Northwind;Trusted_Connection=True;MultipleActiveResultSets=true"

# References

https://www.youtube.com/watch?v=hpLvXNASyTI
