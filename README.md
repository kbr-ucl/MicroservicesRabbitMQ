# MicroservicesRabbitMQ

Open a powershell terminal window i the folder containing MicroservicesRabbitMQ.sln

Start SQL server:

```powershell
docker run --name tempsql -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=SqlPassword#1234' -p 11433:1433 -v .\.containers\database:/var/opt/mssql/data -d mcr.microsoft.com/mssql/server:latest
```



Set ProductOwner.Microservice as startup project

In Packet Manager Console:

- Default project: ProductOwner.Microservice

- Update-Database

Set ProductOwner.Microservice as startup project

In Packet Manager Console:

- Default project: ProductOwner.Microservice

- Update-Database

In powershell terminal window

Delete database server in Docker

```c#
docker rm --force tempsql
```

In powershell terminal window

Start Docker Compose

```powershell
docker compose up
```

Check that all is started in Docker Desktop

If product-offer-mq does not start:

```powershell
docker compose down
docker compose up
```

Open browser: 

- http://localhost:8080/swagger
- http://localhost:8090/swagger
- http://localhost:15672/  login: guest guest



You are up and running :-)



