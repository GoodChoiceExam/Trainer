# Trainer

Microservice der håndterer personlige trænere og bookinger i FitLife.

## Hvad servicen kan

- Oprette, hente og slette trænerprofiler
- Booke en session med en træner — afviser automatisk hvis det valgte tidspunkt allerede er optaget
- Afmelde en eksisterende booking
- Hente egne bookinger på tværs af trænere
- Hente hvilke timer en bestemt træner er booket på en given dato

Alle endpoints kræver et gyldigt JWT token udstedt af Identity-servicen.

## Struktur

- `Controllers/` — HTTP endpoints
- `Services/` — forretningslogik
- `Repositories/` — MongoDB-adgang
- `Models/` — domæneobjekter (`PersonalTrainer`, `TrainerBooking`, m.fl.)
- `FitLife.Trainer.Tests/` — unit tests med NUnit