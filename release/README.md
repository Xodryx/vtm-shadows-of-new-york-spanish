# Shadows of New York en español

Traducción no oficial al español (de España) de *Vampire: The Masquerade – Shadows of New
York*. Añade **Español** al selector de idiomas del juego. No modifica ningún archivo del
juego.

## Instalación

Todo se hace en la carpeta del juego, la que contiene `VtM Shadows of New York.exe`. En
Steam se abre así: botón derecho sobre el juego → **Administrar → Ver archivos locales**.
En una instalación por defecto es:

```
C:\Program Files (x86)\Steam\steamapps\common\Vampire The Masquerade - Shadows of New York
```

1. **Descarga BepInEx.** En <https://github.com/BepInEx/BepInEx/releases>, abre una
   versión **5.4.x** (no la 6, que es experimental) y descarga
   `BepInEx_win_x64_5.4.*.zip`. Tiene que ser **x64**, no x86: es el error más común.
2. **Descomprímelo en la carpeta del juego**, de forma que `winhttp.dll` y la carpeta
   `BepInEx` queden justo al lado de `VtM Shadows of New York.exe`. Si se han quedado
   dentro de una subcarpeta (por ejemplo `BepInEx_win_x64_5.4.23\`), sácalos un nivel.
3. **Abre el juego una vez y ciérralo.** BepInEx crea sus carpetas en ese primer
   arranque; dentro del juego no verás nada distinto, es normal. Si ha funcionado, ahora
   existe `BepInEx/LogOutput.log`.
4. **Descomprime el archivo de esta traducción en la misma carpeta. Ya incluye la ruta
   `BepInEx/plugins/SoNY-ESP/`, así que todo queda en su sitio; si Windows pregunta si
   quieres combinar carpetas, di que sí.
5. Abre el juego, ve a los ajustes y elige **Español**.

Si no aparece `BepInEx/LogOutput.log` tras el primer arranque, casi siempre es porque
`winhttp.dll` no está junto al ejecutable o porque se descargó la versión x86. Algunos
antivirus bloquean `winhttp.dll` la primera vez: en ese caso, añade una excepción para la
carpeta del juego.

Si todo ha ido bien, tendrás esto:

```
Vampire The Masquerade - Shadows of New York/
├─ winhttp.dll                  ← BepInEx
├─ doorstop_config.ini          ← BepInEx
├─ BepInEx/
│  ├─ LogOutput.log
│  └─ plugins/
│     └─ SoNY-ESP/
│        ├─ SoNY.Esp.dll
│        ├─ es.csv
│        └─ ui_es.csv
└─ VtM Shadows of New York.exe
```

## Qué está traducido

- **Todos los diálogos**: 5699 líneas, ambos finales y todas las ramas opcionales.
- **Menús, nombres, rasgos y misiones**: el juego ya traía esta parte en español oficial,
  escondida. La traducción la activa tal cual.
- **Diccionario**: los títulos son oficiales; las 42 descripciones que el juego dejó sin
  traducir están traducidas aquí.

La terminología sigue el español oficial del propio juego y, donde este no llega, el de
*Vampire: The Masquerade – Bloodlines 2*: Vástago, chiquilla, sire, Mascarada, Muerte
Definitiva, chupóptero...

## Aviso: hecha con IA

**El texto y el plugin los ha escrito un asistente de IA (Claude) bajo supervisión
humana.** Nadie se ha pasado todavía el juego en español para revisarlo, así que tómalo
como un primer borrador jugable. La terminología está contrastada con fuentes oficiales; lo
que puede fallar es el tono, alguna concordancia o algún texto que se salga del cuadro.

Cualquier línea que el plugin no encuentre se queda en inglés: no se rompe nada y las
partidas guardadas no se ven afectadas.

## Cómo ayudar a mejorarla

Toda corrección es bienvenida. Lo más útil es una captura y la frase tal como debería
quedar.

Si quieres revisar a fondo:

1. Abre `BepInEx/config/sony.esp.cfg` y, en la sección `[1. Dump]`, pon `Enabled = true`.
2. Abre el juego hasta el menú principal y ciérralo.
3. En `BepInEx/plugins/SoNY-ESP/` aparecerá **`review_es.csv`**: cada línea con el inglés
   (columna EN) y el español (columna ES) lado a lado. Se abre con LibreOffice Calc,
   Excel o Google Sheets (codificación UTF-8, separado por comas).
4. Corrige la columna ES. Para probar los cambios en el juego, guarda el archivo como
   `es.csv` (en CSV UTF-8) en lugar del original: el plugin lee la última columna.
5. Vuelve a poner `Enabled = false` cuando termines.

Las reglas de estilo y el glosario están en `GUIA_DE_ESTILO.md`, en el repositorio del
proyecto.

## Desinstalación

Primero vuelve a poner el juego en **inglés** desde los ajustes. Después borra la carpeta
`BepInEx/plugins/SoNY-ESP/`. Para quitar también BepInEx, borra `winhttp.dll`,
`doorstop_config.ini`, `.doorstop_version` y la carpeta `BepInEx`.

No se modifica ningún archivo del juego, así que no hay nada que restaurar. No toques
`UnityPlayer.dll` ni `WinPixEventRuntime.dll`: son del juego.

## Comprobar que se ha cargado

`BepInEx/LogOutput.log` debería contener:

```
[Info   :Shadows of New York - Spanish] Español added to the language selector.
[Info   :Shadows of New York - Spanish] es.csv: <n> translations loaded.
[Info   :Shadows of New York - Spanish] Localization.language = 'es[Female]'
```
