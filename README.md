# D&D Initiative Tracker

A WPF (.NET 8) campaign-based D&D 5e initiative tracker with creature import from the community 5e API.

## Projects

| Project | Type | Purpose |
|---|---|---|
| `DnDInitiativeTracker` | WPF App (.NET 8 Windows) | UI, MVVM shell, drag-drop initiative list |
| `DnDInitiativeTracker.Core` | Class Library (.NET 8) | Domain models, service contracts, JSON repositories |

## Features

- **Campaign management** — Create/delete named campaigns, each stored in its own subfolder
- **Encounter tracking** — Multiple encounters per campaign, persisted as JSON
- **Manual initiative entry** — Players roll physically; GM types in the rolls
- **Auto-sort + drag reorder** — Combatants auto-sort by roll descending; drag any row to override order
- **Creature import** — Search and import creatures from the community 5e API (dnd5eapi.co) on demand
- **D&D Beyond character images** — Direct image URLs stored on `CharacterProfile.DirectImageUrl`

## Data Layout

All data is stored project-local next to the built executable:

```
Data/
  Campaigns/
    {campaign-slug}/
      campaign.json
      encounters/
        {encounter-id}.json
      assets/
  CompendiumCache/        ← reserved for future creature caching
```

## Running

```powershell
dotnet restore
dotnet build DnDInitiativeTracker.sln
dotnet run --project DnDInitiativeTracker/DnDInitiativeTracker.csproj
```

## Architecture

```
DnDInitiativeTracker (WPF)
  ViewModels/
    ShellViewModel        ← coordinates campaign → encounter loading
    MainViewModel         ← campaign CRUD
    EncounterViewModel    ← encounter/combatant CRUD + initiative
    CombatantViewModel    ← per-row observable wrapper
  Converters/
    BoolToVisibilityConverter
    InverseBoolToVisibilityConverter
  MainWindow.xaml         ← 3-column layout + drag-drop code-behind

DnDInitiativeTracker.Core (Class Library)
  Models/
    Campaign, Encounter, Combatant, CharacterProfile, CreatureTemplate
  Services/
    IInitiativeOrderingService / InitiativeOrderingService
    ICreatureCatalogService / FiveBitsCreatureCatalogService
  Repositories/
    ICampaignRepository / JsonCampaignRepository
    IEncounterRepository / JsonEncounterRepository
```

