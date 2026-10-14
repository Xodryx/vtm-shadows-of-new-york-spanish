# Vampire: The Masquerade – Shadows of New York en español

Traducción no oficial al español de España. Añade **Español** al selector de idiomas del
juego mediante un plugin de BepInEx, sin modificar ningún archivo del juego.

- **Para jugar:** descarga el zip de la última *release* y sigue
  [release/README.md](release/README.md).
- **Para corregir:** lee [GUIA_DE_ESTILO.md](GUIA_DE_ESTILO.md) y la sección *Cómo ayudar*
  de [release/README.md](release/README.md).

> **Hecha con IA.** Los diálogos y el plugin los ha escrito un asistente de IA (Claude)
> bajo supervisión humana. La terminología está contrastada con fuentes oficiales, pero
> nadie se ha pasado todavía el juego en español. Las correcciones son bienvenidas.

## Qué hay dentro

| | |
|---|---|
| `release/` | Lo que se distribuye: el plugin, `es.csv` (diálogos) y `ui_es.csv` (interfaz). |
| `blocks/*.json` | La traducción por bloques: `{"ConvID/EntryID/Campo": "texto"}`. Es la fuente; `es.csv` se genera a partir de aquí. |
| `ui_manual_es.json` | Las 42 descripciones del diccionario que el juego no traía en español. |
| `glossary_es.json` | Forma oficial de cada término marcado como `[ID;término]` en el texto. |
| `GUIA_DE_ESTILO.md` | Glosario, voz de Julia, tratamiento (tú/usted) y tipografía. |
| `plugin/` | Código del plugin (C#). Todo lo que depende del idioma está en `LanguageInfo.cs`. |
| `apply.py` | Une los bloques en `es.csv` y comprueba marcadores, saltos de línea y longitud. |
| `reading_order.py` | Ordena las líneas de cada conversación tal y como se leen en el juego. |
| `build_ui.py` | Genera `ui_es.csv`. |
| `tools/` | Extracción de la localización oficial de Bloodlines 2, usada como referencia terminológica. |

## Cómo se trabaja

El texto original no está en el repositorio: es el guion completo del juego y no es
nuestro para redistribuirlo. Cada uno lo genera con su propia copia:

1. Instala el plugin y, en `BepInEx/config/sony.esp.cfg`, pon `[1. Dump] Enabled = true`.
2. Abre el juego hasta el menú principal. El plugin escribe `dump_en.csv`,
   `dump_ui_en.csv`, `links.csv` y `review_es.csv` en `BepInEx/plugins/SoNY-ESP/`.
3. Copia `dump_en.csv`, `dump_ui_en.csv` y `links.csv` a la raíz del proyecto.

Después:

```
python -X utf8 reading_order.py 114 --todo      # líneas de la conversación 114, en orden de lectura
python -X utf8 apply.py --check blocks/x.json   # comprobar un bloque sin escribir nada
python -X utf8 apply.py blocks/*.json           # regenerar es.csv en la carpeta del plugin
python -X utf8 build_ui.py                      # regenerar ui_es.csv
```

El plugin vuelve a leer los CSV en cada arranque, así que los cambios se prueban
reiniciando el juego.

## Otro idioma

El plugin no tiene nada atado al español fuera de `plugin/LanguageInfo.cs`. El juego ya
conoce DE, ES, FR, PL, PT, RU, CN, JP, TR e IT; para otro idioma basta con cambiar ese
archivo, recompilar y traducir los CSV. Comprueba antes que las fuentes del juego tienen
los caracteres necesarios: el latín extendido funciona, el chino y el japonés no.
