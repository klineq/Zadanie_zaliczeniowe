# Symulator Logistyki Bazy Kosmicznej

**Autor:** Kewin Jara (83098)  
**Przedmiot:** Programowanie obiektowe (projekt zaliczeniowy na 5.0)

Konsolowa symulacja bazy orbitalnej: maszyny wydobywają rudę, huta ją przetapia, energia i temperatura są monitorowane, a stan można zapisać do pliku. Całość w C# / .NET 8.

---

## Projekt wykonany na 5.0 (BDB)



1. **Spectre.Console** — zamiast suchego `Console.WriteLine` jest kolorowe menu, tabele maszyn i magazynu, panele statusu oraz paski postępu (np. przy skanie planety i po ticku produkcyjnym).
2. **Asynchroniczny JSON** — zapis i odczyt stanu bazy (`spacebase_save.json`) przez `async/await` i `System.Text.Json`, plus async skan planety z opóźnieniem symulującym długą operację.



---

## Jak to odpalić

Wymagania: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
cd SpaceBaseLogistics
dotnet restore
dotnet build
dotnet run
```

Albo z poziomu folderu rozwiązania:

```bash
dotnet run --project SpaceBaseLogistics
```

Po starcie od razu widać menu Spectre.Console. Na start ładuje się demo (Miner + Smelter). Przydatne opcje do pokazania na obronie:

- **Wykonaj cykl pracy (tick)** — polimorfizm `ProcessTick`, magazyn, paski ciepła
- **Symuluj awarię** — event `OnOverheat`
- **Zapisz / Wczytaj stan bazy** — async JSON
- **Diagnostyka refleksyjna** — refleksja właściwości + atrybut `[HighVoltage]`

---

## Gdzie szukać wymagań 1–12

| Wymóg | Opis | Plik(i) |
|------:|------|---------|
| 1 | Klasy | `Models/Resource.cs`, `Machine.cs`, `Services/SpaceBaseSimulator.cs`, … |
| 2 | Konstruktory | `Models/Machine.cs`, `Miner.cs`, `Smelter.cs` |
| 3 | Właściwości / indeksatory | `Core/Inventory.cs` (`this[string]`), użycie w `UI/ConsoleAppUi.cs` |
| 4 | Statyczne | `Core/BaseEnergyTracker.cs` |
| 5 | Dziedziczenie | `Models/Miner.cs`, `Smelter.cs` → `Machine` |
| 6 | Polimorfizm | `Models/Machine.cs` (`virtual ProcessTick`), nadpisania w Miner/Smelter |
| 7 | Interfejsy / abstrakcja | `Models/IConnectable.cs`, abstrakcyjna `Machine` |
| 8 | Generyki / kolekcje | `Core/ProductionQueue.cs`, `List<T>`, `Dictionary<K,V>` w symulatorze |
| 9 | Delegacje / zdarzenia | `Models/Machine.cs` (`event OnOverheat`), `Services/SpaceBaseSimulator.cs` |
| 10 | Przeciążanie operatorów | `Models/ResourceStack.cs` (`operator +`), menu w `UI/ConsoleAppUi.cs` |
| 11 | Async/await | `Services/StatePersistenceService.cs`, `PlanetScanService.cs`, `Program.cs` |
| 12 | Refleksja | `Services/DiagnosticReflectionService.cs`, `Models/HighVoltageAttribute.cs` |

**Dodatek (ocena 5.0):** `UI/ConsoleAppUi.cs` (Spectre.Console), async JSON w `StatePersistenceService.cs`.

---

## Struktura folderów

```
SpaceBaseLogistics/
├── Core/          # magazyn, kolejka generyczna, licznik energii
├── Models/        # maszyny, surowce, interfejsy
├── Services/      # symulacja, zapis JSON, skan, refleksja
├── UI/            # menu Spectre.Console
└── Program.cs     # punkt wejścia
```

