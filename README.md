# UrlShortener em C#

Biblioteca em C# (.NET 8) que encurta URLs usando codificação base62 sobre ids sequenciais, com armazenamento em memória ou em arquivo JSON, mais uma CLI pequena para uso no terminal. O objetivo é mostrar, de forma enxuta, o núcleo de um serviço como bit.ly: gerar códigos curtos, deduplicar URLs e resolver o código de volta para o destino.

## Por que este projeto

"Projete um encurtador de URLs" é uma das perguntas de system design mais frequentes em entrevistas de big techs. Este projeto implementa a parte algorítmica e de modelagem que costuma ser discutida no quadro: conversão de base, geração de ids, idempotência (a mesma URL sempre gera o mesmo código), validação de entrada e uma abstração de armazenamento (`IUrlStore`) que poderia ser trocada por Redis ou um banco relacional. É também um bom exemplo de código pequeno, testável e sem dependências externas.

## Recursos

- Codificação/decodificação base62 (`0-9A-Za-z`) para `ulong`, com detecção de overflow e caracteres inválidos.
- Ids sequenciais, thread-safe, com *offset* configurável para que os códigos tenham tamanho mínimo e fiquem menos previsíveis.
- Deduplicação: encurtar duas vezes a mesma URL devolve o mesmo código.
- Validação: apenas URLs absolutas `http`/`https`; a URL é normalizada com `Uri.AbsoluteUri`.
- Dois armazenamentos: `InMemoryUrlStore` e `JsonFileUrlStore` (gravação atômica via arquivo temporário + `File.Move`).
- CLI com os comandos `shorten` e `resolve`.

## Como executar

Pré-requisito: .NET 8 SDK.

```bash
dotnet build
dotnet run --project src/UrlShortener.Cli -- shorten https://example.com/um/caminho/muito/longo --store urls.json
# https://sho.rt/100001
dotnet run --project src/UrlShortener.Cli -- resolve 100001 --store urls.json
# https://example.com/um/caminho/muito/longo
```

Uso como biblioteca:

```csharp
var shortener = new Shortener(new JsonFileUrlStore("urls.json"), "https://sho.rt/", offset: 916_132_832);
string link = shortener.ShortLink("https://example.com/a");
string original = shortener.Resolve(link);
```

## Como testar

```bash
dotnet test
```

Os testes cobrem valores conhecidos de base62, round-trip com 5 mil números aleatórios, rejeição de entradas inválidas, idempotência, concorrência (2 mil encurtamentos paralelos com códigos únicos) e persistência entre execuções.

## Decisões de projeto

- **Contador + base62 em vez de hash.** Um hash truncado exige tratamento de colisões; um contador monotônico garante unicidade sem retentativas. A troca é a previsibilidade, mitigada em parte pelo `offset`. Em um sistema distribuído, o contador viria de um alocador de faixas (ex.: Snowflake ou faixas reservadas por nó).
- **Complexidade.** `Encode`/`Decode` são O(k), com k = número de dígitos (no máximo 11 para 64 bits). `Shorten` e `Resolve` são O(1) amortizado, com dois dicionários (id para URL e URL para id).
- **Capacidade.** 62^7 ≈ 3,5 trilhões de códigos com 7 caracteres.
- **`IUrlStore`.** Isola a persistência, o que permite testar a lógica sem I/O e trocar o backend.
- **JSON reescrito inteiro a cada escrita.** Simples e atômico, mas O(n) por inserção. Adequado para demonstração; um log append-only seria o próximo passo (veja o projeto KeyValueStore).

## Estrutura de pastas

```
UrlShortener/
├── UrlShortener.sln
├── src/
│   ├── UrlShortener/        # biblioteca (Base62, Shortener, stores)
│   └── UrlShortener.Cli/    # CLI
└── tests/
    └── UrlShortener.Tests/  # testes xUnit
```

## Limitações conhecidas

- Não há expiração de links, aliases personalizados nem contagem de cliques.
- O `JsonFileUrlStore` não coordena escritas entre processos diferentes.
- Não há servidor HTTP de redirecionamento; o foco é a biblioteca e a CLI.
- O `offset` não é segredo: não use os códigos como proteção contra enumeração.

## Licença

MIT. Veja o arquivo [LICENSE](LICENSE).
