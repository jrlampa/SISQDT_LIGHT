# Estágio de Build e Testes Automatizados (Zero Custo / Docker First)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia arquivos de solução e projetos
COPY QdtCqts.slnx ./
COPY src/QdtCqts.Domain/*.csproj ./src/QdtCqts.Domain/
COPY src/QdtCqts.Application/*.csproj ./src/QdtCqts.Application/
COPY src/QdtCqts.Calculation.Abstractions/*.csproj ./src/QdtCqts.Calculation.Abstractions/
COPY src/QdtCqts.Calculation.Qdt/*.csproj ./src/QdtCqts.Calculation.Qdt/
COPY src/QdtCqts.Calculation.Cqts/*.csproj ./src/QdtCqts.Calculation.Cqts/
COPY src/QdtCqts.Infrastructure.Sqlite/*.csproj ./src/QdtCqts.Infrastructure.Sqlite/
COPY src/QdtCqts.Infrastructure.ExcelEvidence/*.csproj ./src/QdtCqts.Infrastructure.ExcelEvidence/
COPY src/QdtCqts.Infrastructure.Parity/*.csproj ./src/QdtCqts.Infrastructure.Parity/
COPY tests/QdtCqts.Tests.Domain/*.csproj ./tests/QdtCqts.Tests.Domain/
COPY tests/QdtCqts.Tests.Topology/*.csproj ./tests/QdtCqts.Tests.Topology/
COPY tests/QdtCqts.Tests.Import/*.csproj ./tests/QdtCqts.Tests.Import/
COPY tests/QdtCqts.Tests.Parity/*.csproj ./tests/QdtCqts.Tests.Parity/

# Restauração das dependências
RUN dotnet restore tests/QdtCqts.Tests.Domain/QdtCqts.Tests.Domain.csproj \
    && dotnet restore tests/QdtCqts.Tests.Topology/QdtCqts.Tests.Topology.csproj \
    && dotnet restore tests/QdtCqts.Tests.Import/QdtCqts.Tests.Import.csproj \
    && dotnet restore tests/QdtCqts.Tests.Parity/QdtCqts.Tests.Parity.csproj

# Copia todo o código-fonte restante
COPY src/ ./src/
COPY tests/ ./tests/

# Execução da suíte completa de testes
CMD ["dotnet", "test", "--logger", "console;verbosity=normal"]
