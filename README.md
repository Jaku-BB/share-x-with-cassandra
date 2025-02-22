# ShareX

Projekt zaliczeniowy na przedmiot *Bazy danych II* na [Akademii Górnośląskiej](https://www.gwsh.pl) w Katowicach.

Aplikacja internetowa **ShareX** pozwala użytkownikom na proste i szybkie udostępnianie plików.

Użytkownik chcąc udostępnić plik, przesyła go za pomocą formularza, a aplikacja generuje link, który pozwala na jego
pobranie.

## Stos technologiczny

- [ASP.NET Core 8](https://learn.microsoft.com/en-us/aspnet/core/getting-started/?view=aspnetcore-8.0)
- [Apache Cassandra](https://cassandra.apache.org/_/index.html)
- [Swagger](https://swagger.io)
- [Next.js](https://nextjs.org)

## Struktura projektu

- `server` - API
- `client` - front aplikacji

## Wymagania

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)
- [Node.js](https://nodejs.org)

## Uruchomienie

### Baza danych

Aplikacja wykorzystuje klaster Apache Cassandra składający się z dwóch węzłów. Aby uruchomić bazę danych:

```bash
cd server
docker-compose up -d
```

Sprawdź status klastra:
```bash
docker-compose ps
```

Oba kontenery powinny mieć status "healthy" przed uruchomieniem API.

### Konfiguracja

Domyślnie aplikacja łączy się z Cassandrą na `localhost:9042` i `localhost:9043`. Możesz zmienić te ustawienia w pliku `appsettings.json`:

```json
{
  "Cassandra": {
    "ContactPoints": ["host1:port1", "host2:port2"],
    "Username": "opcjonalny_użytkownik",
    "Password": "opcjonalne_hasło"
  }
}
```

### API

```bash
cd server
dotnet run
```

API będzie dostępne pod adresem `https://localhost:7042` lub `http://localhost:5042`.

### Front

Należy pamiętać o uzupełnieniu pliku `.env` na podstawie `.env.example`.

```bash
cd client
npm install
npm run dev
```

## Funkcjonalności

- Przesyłanie plików do 1 6MB
- Automatyczna replikacja danych między węzłami Cassandry
- Wysoka dostępność dzięki klastrowi Cassandra
- Generowanie unikalnych identyfikatorów dla plików
- Pobieranie plików przez API
- Sprawdzanie istnienia plików

## Autor

- [Jakub Bukała](https://github.com/Jaku-BB)