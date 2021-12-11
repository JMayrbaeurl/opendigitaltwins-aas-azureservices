# IO.Swagger - ASP.NET Core 2.0 Server

An exemplary interface combination for the use case of an Asset Administration Shell Repository

## Run

Linux/OS X:

```
sh build.sh
```

Windows:

```
build.bat
```

## Run in Docker

```
cd src/IO.Swagger
docker build -t io.swagger .
docker run -p 5000:5000 io.swagger
```
