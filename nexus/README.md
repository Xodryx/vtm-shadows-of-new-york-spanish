# Testi della pagina Nexus Mods

Un file per campo del modulo di caricamento. Sono file di testo puro in BBCode: si apre,
si seleziona tutto, si incolla. Non sono `.md` apposta, altrimenti l'anteprima Markdown li
deformerebbe.

| File | Campo sul modulo |
|---|---|
| `01-description.txt` | Description |
| `02-installation.txt` | Installation instructions |
| `03-features.txt` | Main features |
| `04-requirements.txt` | Requirements |
| `05-shout-outs.txt` | Shout outs |

## Campi da compilare a mano

| Campo | Valore |
|---|---|
| Mod Name | `Traducción al español - Spanish Translation` |
| Summary | vedi sotto |
| Version | `0.1.0` |
| Language | Spanish |
| Category | Translations, oppure Miscellaneous se non esiste |
| Tag IA | **AI-Generated Content**, non "AI Assisted" (obbligatorio per policy) |

Summary, massimo 250 caratteri:

    Traducción completa al español de Shadows of New York: las 5699 líneas, diccionario, nombres y misiones. Plugin de BepInEx: no modifica ningún archivo del juego. Activa además el español oficial que el juego traía escondido.

## A ogni nuova versione

Il numero di battute compare in `01-description.txt`, `03-features.txt` e nel Summary.
Il numero di versione va aggiornato nel campo Version e nel tag git.

Caricare sempre lo zip costruito dalla CI di GitHub, mai uno ricostruito a mano.
