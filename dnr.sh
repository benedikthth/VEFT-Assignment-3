#!/bin/bash
cd Api/
dotnet restore
cd ..
cd Models
dotnet restore
cd ..
cd Repositories
dotnet restore
cd ..
cd Services
dotnet restore
echo "Done"
