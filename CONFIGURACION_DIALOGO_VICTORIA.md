# INSTRUCCIONES: Configurar Diálogo de Victoria del Jefe

## Archivos Modificados/Creados:
1. **BossController.cs** - Agregado sistema de diálogo de victoria
2. **VictoryDialog.cs** - Nuevo script especializado para diálogo de victoria

---

## PASOS PARA CONFIGURAR EN UNITY:

### 1. Crear el GameObject del Diálogo de Victoria

1. En la escena del jefe, crea un GameObject vacío llamado **"VictoryDialogTrigger"**
2. Agrégale el componente **VictoryDialog** (en lugar de Dialog_2)
3. **IMPORTANTE:** Desactiva este GameObject por defecto (checkbox desactivado en el Inspector)

### 2. Configurar el VictoryDialog

En el Inspector del **VictoryDialogTrigger**, configura:

#### **Dialogue Content:**
- **Lines**: Escribe las líneas de felicitación, ejemplo:
  ```
  Línea 1: "¡Felicidades, valiente aventurero!"
  Línea 2: "Has derrotado al temible jefe final."
  Línea 3: "Tu coraje y habilidad han salvado estas tierras."
  Línea 4: "Gracias por jugar."
  ```
- **NPC Name**: "Narrador" (o el nombre que quieras)

#### **UI References (IMPORTANTE - ASIGNAR ESTAS REFERENCIAS):**
- **Dialog Panel Reference**: ?? Arrastra aquí el Panel de UI del diálogo (debe estar ACTIVO en la jerarquía para poder arrastrarlo, luego lo desactivas)
- **Dialog Text Reference**: ?? Arrastra el TextMeshProUGUI donde aparece el texto
- **Name Text Reference**: Arrastra el TextMeshProUGUI del nombre (opcional)

**TRUCO IMPORTANTE:** Si el panel está desactivado y no puedes arrastrarlo:
1. Activa temporalmente el panel en la jerarquía
2. Arrastra las referencias al VictoryDialog
3. Desactiva el panel de nuevo

#### **Dialogue Settings:**
- **Text Speed**: 0.05 (o el valor que prefieras - más bajo = más rápido)
- **NPC Voice**: Sonido de voz del narrador (opcional)
- **Sound Every N Letters**: 2
- **Close Dialogue On Finish**: ? (marcado)
- **Freeze Player**: ? (marcado)

#### **Victory Settings:**
- **Menu Scene Name**: "MenuInicial" (o el nombre exacto de tu escena de menú)
- **Delay Before Menu**: 2 (segundos de espera después del diálogo)
- **Disable Player Movement**: ? (marcado)

#### **Audio Settings:**
- **Audio Mixer Group**: Arrastra aquí el grupo "Master Volume" de tu Audio Mixer
  - Ve a tu Audio Mixer (ventana Audio Mixer)
  - Encuentra el grupo "Master" o "Master Volume"
  - Arrástralo al campo Audio Mixer Group
- **Voice Volume**: 0.3 (valor entre 0 y 1 - más bajo que el volumen normal para que no esté tan alto)
  - 0.3 = Volumen bajo (recomendado para victoria)
  - 0.5 = Volumen medio
  - 0.7 = Volumen normal (puede ser muy alto)
  - 1.0 = Volumen máximo

**NOTA:** El AudioSource se crea automáticamente si asignas un NPC Voice. Si quieres más control, agrega manualmente un AudioSource al GameObject antes.

### 3. Configurar el BossController

En el Inspector del **Jefe**, configura:

#### **Victory Dialog:**
- **Victory Dialog**: Arrastra aquí el componente **Dialog_2** del GameObject **VictoryDialogTrigger** 
  (No el GameObject completo, sino el componente Dialog_2/VictoryDialog)
- **Tiempo Antes De Dialogo**: 5 (segundos después de morir antes de mostrar el diálogo)
- **Nombre Escena Menu**: "MenuInicial" (debe coincidir con el nombre real de tu escena)

---

## VERIFICACIÓN DE CONFIGURACIÓN:

### ? Checklist antes de probar:

1. [ ] El GameObject "VictoryDialogTrigger" existe y está **desactivado**
2. [ ] El VictoryDialog tiene el componente VictoryDialog (o Dialog_2) asignado
3. [ ] Las **Lines** tienen al menos 1 línea de texto
4. [ ] **Dialog Panel Reference** está asignado (no es NULL)
5. [ ] **Dialog Text Reference** está asignado (no es NULL)
6. [ ] El BossController tiene el **Victory Dialog** asignado
7. [ ] El nombre de la escena del menú coincide exactamente con Build Settings
8. [ ] **Audio Mixer Group** está asignado (para controlar el volumen desde el Master Volume)
9. [ ] **Voice Volume** está configurado a un nivel cómodo (0.3 recomendado, no usar 0.7 porque está muy alto)

---

## CÓMO FUNCIONA:

1. **Cuando el jefe muere:**
   - Se ejecuta la animación de muerte y el efecto de "derretirse"
   - Espera 5 segundos (configurable con `tiempoAntesDeDialogo`)

2. **Aparece el diálogo de victoria:**
   - BossController activa el GameObject del VictoryDialog
   - BossController llama a `ShowVictoryDialog()`
   - El panel se activa automáticamente
   - El jugador queda congelado para leer
   - Las líneas aparecen con efecto de escritura

3. **El jugador avanza el diálogo:**
   - Presionando E, Espacio o Click izquierdo
   - Al terminar todas las líneas, el diálogo se cierra

4. **Después del diálogo:**
   - Espera 2 segundos adicionales (configurable)
   - Carga automáticamente la escena del menú principal

---

## DEBUG - SI NO APARECE EL PANEL:

### Paso 1: Verificar en la Consola

Cuando el jefe muere, deberías ver estos mensajes en orden:

```
[BossController] ¡Jefe derrotado! Tiempo vivo: X.Xs, Vida restante: 0
[BossController] Animación de muerte completada
[BossController] Mostrando diálogo de victoria
[BossController] ShowVictoryDialog() llamado correctamente
[VictoryDialog] ShowVictoryDialog() llamado
[VictoryDialog] Iniciando con X líneas
[VictoryDialog] ActivateVictoryDialogCoroutine iniciado
[VictoryDialog] Congelando jugador
[VictoryDialog] Activando dialogPanelReference
[VictoryDialog] ForceStartDialogue llamado
[VictoryDialog] Activando panel en ForceStartDialogue
[VictoryDialog] Estableciendo nombre: Narrador
[VictoryDialog] dialogTextReference encontrado, iniciando TypeLineVictory
```

### Paso 2: Identificar el error

Si ves estos errores, significa que falta configurar algo:

- **`dialogPanelReference es NULL!`** ? No asignaste el Panel en el Inspector
- **`dialogTextReference es NULL!`** ? No asignaste el TextMeshProUGUI en el Inspector
- **`No hay líneas de diálogo configuradas`** ? Falta agregar texto en Lines

### Paso 3: Soluciones comunes

| Problema | Solución |
|----------|----------|
| El panel no aparece | Verifica que Dialog Panel Reference esté asignado |
| No hay texto | Verifica que Dialog Text Reference esté asignado |
| El GameObject no se activa | Verifica que Victory Dialog esté asignado en BossController |
| Va directo al menú | Verifica que Lines tenga al menos 1 línea de texto |

---

## TESTING:

Para probar rápidamente:
1. Activa **Mostrar Mensajes Debug** en BossController
2. Reduce `tiempoDeVida` del jefe a 10 segundos
3. O reduce su `vidaMaxima` a 50
4. Inicia el juego y derrota al jefe
5. Observa la consola para ver los mensajes de debug
6. Verifica que:
   - Aparece el diálogo después de 5 segundos
   - Puedes avanzar las líneas
   - Al terminar, carga el menú después de 2 segundos

---

## PERSONALIZACIÓN:

### Cambiar tiempos:
- **Espera antes del diálogo**: `BossController.tiempoAntesDeDialogo`
- **Espera después del diálogo**: `VictoryDialog.delayBeforeMenu`
- **Velocidad de escritura**: `VictoryDialog.textSpeed` (0.05 = rápido, 0.1 = lento)

### Cambiar comportamiento:
- **No congelar jugador**: Desmarca `VictoryDialog.disablePlayerMovement`
- **Ir directo al menú sin diálogo**: Deja `victoryDialog` vacío en BossController

### Ajustar audio:
- **Volumen muy alto**: Reduce `VictoryDialog.voiceVolume` (prueba con 0.2 o 0.3)
- **Volumen muy bajo**: Aumenta `VictoryDialog.voiceVolume` (prueba con 0.5 o 0.6)
- **No se escucha**: Verifica que Audio Mixer Group esté asignado y que el Master Volume no esté en -80dB
- **Sonido se corta**: Reduce `soundEveryNLetters` a 3 o 4 para que suene menos frecuente

---

## COMPARACIÓN DE VOLÚMENES:

Para referencia, aquí están los volúmenes típicos en el juego:

| Elemento | Volumen Típico | Razón |
|----------|---------------|-------|
| Efectos de sonido | 0.7 - 1.0 | Deben destacar |
| Voces de NPCs normales | 0.5 - 0.7 | Nivel medio |
| **Diálogo de Victoria** | **0.3 - 0.4** | Más bajo para no saturar |
| Música de fondo | 0.3 - 0.5 | Ambiente, no debe molestar |

**RECOMENDACIÓN:** Usa `0.3` para el diálogo de victoria. Si aún está muy alto, prueba con `0.2`.

---

¡Listo! Ahora el panel debería activarse correctamente cuando el jefe muere. ???

**RECORDATORIO IMPORTANTE:** Asegúrate de que las referencias **Dialog Panel Reference** y **Dialog Text Reference** estén asignadas en el VictoryDialog, ¡este es el error más común!
