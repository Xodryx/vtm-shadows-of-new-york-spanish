# Shadows of New York en español

Traducción no oficial al español (de España) de *Vampire: The Masquerade – Shadows of New
York*. Añade **Español** al selector de idiomas del juego. No modifica ningún archivo del
juego.

## Instalación

1. Instala **BepInEx 5.4.x (x64)** en la carpeta del juego, la que contiene
   `VtM Shadows of New York.exe`. Descárgalo de
   <https://github.com/BepInEx/BepInEx/releases> (`BepInEx_x64_5.4.*.zip`) y descomprímelo
   ahí.
2. Abre el juego una vez y ciérralo. BepInEx crea sus carpetas en ese primer arranque.
3. Descomprime el archivo de esta traducción en la misma carpeta. Ya incluye la ruta
   `BepInEx/plugins/SoNY-ESP/`, así que todo queda en su sitio; si Windows pregunta si
   quieres combinar carpetas, di que sí.
4. Abre el juego, ve a los ajustes y elige **Español**.

Si todo ha ido bien, tendrás esto:

```
Vampire The Masquerade - Shadows of New York/
├─ winhttp.dll                  ← BepInEx
├─ BepInEx/
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
