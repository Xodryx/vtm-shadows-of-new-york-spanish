# Traduzione spagnola di Shadows of New York — stato

Aggiornato: 14 ottobre 2026

## Fatto

- **Plugin funzionante.** "Español" compare nel selettore lingue (verificato a schermo) e
  il log conferma 5.699 traduzioni caricate e applicate nel campo `es[Female]`.
- **Dialoghi completi: 5.699 battute su 5.699.** 60 tradotte a mano come pilota di tono,
  il resto da 8 traduttori in parallelo con guida, glossario e la versione italiana come
  riferimento per tu/usted.
- **Interfaccia.** Il gioco ha già lo spagnolo ufficiale nascosto per menu, nomi, tratti,
  titoli del dizionario e metà delle missioni: il plugin lo attiva così com'è. Le 42
  descrizioni del dizionario mancanti sono in `ui_manual_es.json`. Le missioni senza
  spagnolo hanno vuoto anche l'inglese, quindi non servono.
- **Uniformazione tra traduttori fatta:** "la Príncipe" per Panhard (31 righe corrette in
  tutto), "Capilla de los Cinco Boroughs" e "Diva's Delight" come nelle tabelle ufficiali,
  Carter che passa al tu nel suo crollo (144/168) come nell'italiano.
- **Controlli meccanici puliti:** nessun marcatore alterato, nessun ¿/¡ mancante, nessuna
  virgoletta inglese. Restano 28 avvisi di lunghezza, tutti su righe brevi.
- **Strumento per i revisori:** con `Dump.Enabled = true` il plugin scrive
  `review_es.csv` con EN ed ES affiancati, generato dalla copia del gioco di chi rivede.

## Da fare

1. **Provarlo giocando**, almeno l'Intro e una scena con la corte, per vedere testi che
   sbordano.
2. **Pubblicazione**, quando lo decidi: repo GitHub (identità noreply, niente
   Co-Authored-By), poi Nexus/Steam. Il README per i giocatori è in `release/README.md`.
3. **Revisione da parte della comunità ispanofona.**

## Note

- Se sono installate entrambe le mod, lo spagnolo viene caricato prima dell'italiano:
  nel selettore "Español" sta in quinta posizione e "Italiano" in sesta.
- Le fonti terminologiche e il metodo di estrazione da Bloodlines 2 sono descritti nella
  memoria di progetto e in `tools/`.
