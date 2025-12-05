# INSTRUCCIONES: Configurar Controles Móviles para Android

## Archivos Modificados/Creados:
1. **PlayerMovementJoystick.cs** - Sistema de controles móviles (NUEVO)
2. **PlayerMovement.cs** - Modificado para soportar input móvil
3. **PlayerCombat.cs** - Agregado método MobileAttack()

---

## CONFIGURACIÓN EN UNITY:

### 1. Preparar el Canvas de UI Móvil

1. **Crear Canvas (si no existe):**
   - Click derecho en Hierarchy ? UI ? Canvas
   - Nombre: "MobileControlsCanvas"
   - Configurar Canvas Scaler:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920x1080
     - Match: 0.5 (balance entre Width y Height)

2. **Verificar que tienes los siguientes elementos:**
   - ? **Fixed Joystick** (del Asset Pack que descargaste)
   - ? **JumpButton** (Button de UI)
   - ? **InteractButton** (Button de UI)
   - ? **AttackButton** (Button de UI)

---

### 2. Configurar el PlayerMovementJoystick

1. **Agregar el script al jugador:**
   - Selecciona el GameObject del Player
   - Add Component ? Player Movement Joystick

2. **Configurar Referencias del Jugador:**
   - **Player Movement**: Arrastrar el componente PlayerMovement del mismo objeto
   - **Player Combat**: Arrastrar el componente PlayerCombat del mismo objeto

3. **Configurar Controles de UI:**
   - **Movement Joystick**: Arrastrar el Fixed Joystick del Canvas
   - **Jump Button**: Arrastrar el botón de salto
   - **Interact Button**: Arrastrar el botón de interacción
   - **Attack Button**: Arrastrar el botón de ataque

4. **Configuración Adicional:**
   - **Joystick Deadzone**: 0.1 (ignorar movimientos pequeños)
   - **Desactivar En PC**: ? (marcado) - Desactiva controles móviles cuando juegas en PC
   - **Mostrar Debug**: ? (opcional) - Para ver logs de los botones

---

### 3. Configurar el Joystick

Tu Fixed Joystick ya debería estar configurado, pero verifica:

1. **Componente Joystick:**
   - Handle Range: 1
   - Dead Zone: 0 (lo controlamos desde PlayerMovementJoystick)
   - Axis Options: Horizontal (solo movimiento lateral)

2. **Posición en el Canvas:**
   - Colócalo en la esquina inferior izquierda
   - Anchor: Bottom-Left
   - Pivot: (0.5, 0.5)

---

### 4. Configurar los Botones

#### Jump Button:
- **Posición**: Esquina inferior derecha
- **Tamaño**: 100x100 (o al gusto)
- **Imagen**: Ícono de salto o flecha hacia arriba
- **IMPORTANTE**: NO necesitas configurar onClick, el script lo hace automáticamente

#### Attack Button:
- **Posición**: Al lado del Jump Button
- **Tamaño**: 100x100
- **Imagen**: Ícono de espada o ataque
- **IMPORTANTE**: NO configurar onClick manualmente

#### Interact Button:
- **Posición**: Centro derecha o donde prefieras
- **Tamaño**: 80x80
- **Imagen**: Ícono de "E" o mano
- **IMPORTANTE**: NO configurar onClick manualmente

---

## CÓMO FUNCIONA:

### Input de Movimiento:
```
Joystick ? PlayerMovementJoystick.GetHorizontalInput()
         ? PlayerMovement.SetMobileInput(horizontal)
         ? PlayerMovement aplica el movimiento
```

### Input de Salto:
```
Jump Button presionado ? PlayerMovementJoystick.OnJumpButtonDown()
                       ? PlayerMovement.MobileJump()
                       ? PlayerMovement ejecuta salto
```

### Input de Ataque:
```
Attack Button presionado ? PlayerMovementJoystick.OnAttackButtonDown()
                         ? PlayerCombat.MobileAttack()
                         ? PlayerCombat ejecuta ataque
```

### Input de Interacción:
```
Interact Button presionado ? PlayerMovementJoystick.OnInteractButtonDown()
                           ? Busca Dialog_2 cercanos
                           ? Busca ChangeScene cercanos
                           ? Busca objetos con tag "Interactable"
                           ? Activa el más cercano
```

**SISTEMA INTELIGENTE:** El botón de Interact detecta automáticamente:
- ? **Dialog_2** - Inicia/avanza diálogos con NPCs
- ? **ChangeScene** - Entra a lugares (pirámides, casas, etc.)
- ? **Objetos Interactuables** - Con tag "Interactable" (opcional)

**NO NECESITAS** crear el tag "Interactable" si solo usas Dialog_2 y ChangeScene.

---

## TOUCH SUPPORT (MÓVIL):

El sistema ahora tiene soporte completo para touch:

### ? Botones de UI
Todos los botones (Jump, Attack, Interact) funcionan con touch automáticamente.

### ? Dialog_1 (Historia/Intro)
- **PC**: Click izquierdo para avanzar
- **Móvil**: Toca la pantalla para avanzar
- **Funciona automáticamente** en ambas plataformas

### ? Menú
Los botones del menú de Unity ya funcionan con touch por defecto.

---

## CONFIGURAR TAG INTERACTABLE (OPCIONAL):

### Opción 1: Crear el Tag (Recomendado)
1. Ve a **Edit ? Project Settings ? Tags and Layers**
2. Click en el **+** en la sección "Tags"
3. Escribe: **Interactable**
4. Asigna este tag a tus objetos interactuables:
   - NPCs con Dialog_2
   - Carteles
   - Cofres
   - Puertas
   - Cualquier objeto con el que el jugador pueda interactuar

### Opción 2: Desactivar el Botón de Interact
Si no usas interacciones en tu juego:
1. En el PlayerMovementJoystick, deja **Interact Button** vacío (None)
2. El botón simplemente no se configurará

### Opción 3: Usar otro sistema
Si tu sistema de interacción es diferente (por ejemplo, usar Dialog_2 directamente):
- El código actual usa try-catch para evitar el error
- El botón no hará nada si el tag no existe
- Modifica el método `EjecutarInteraccion()` según tu necesidad

---

## CÓMO FUNCIONA CADA SISTEMA DE INTERACCIÓN:

### 1. Dialog_2 (NPCs y Carteles)
**PC:** Presiona **E** para iniciar/avanzar diálogo
**Móvil:** Toca el botón **Interact**

**Flujo:**
1. Jugador se acerca al NPC
2. Aparece el marcador de "E"
3. Presiona E (PC) o botón Interact (móvil)
4. El diálogo se inicia
5. Presiona E/Interact/Click para avanzar líneas
6. Al terminar, el diálogo se cierra

### 2. ChangeScene (Entrar a Lugares)
**PC:** Presiona **W** para entrar
**Móvil:** Toca el botón **Interact**

**Ejemplo:** Entrar a la pirámide, casa, cueva, etc.

**Flujo:**
1. Jugador se acerca a la entrada
2. Aparece el marcador de "W"
3. Presiona W (PC) o botón Interact (móvil)
4. Carga la nueva escena

### 3. Dialog_1 (Historia/Intro)
**PC:** Click izquierdo para avanzar
**Móvil:** Toca **cualquier parte de la pantalla**

**Flujo:**
1. El texto empieza a escribirse automáticamente
2. Click/Touch: Completa la línea actual
3. Click/Touch otra vez: Avanza a la siguiente línea
4. Al terminar: Carga la siguiente escena automáticamente

### 4. Menú Principal
**PC:** Click en los botones
**Móvil:** Toca los botones

Los botones de Unity funcionan automáticamente con touch.

---

## SISTEMA DE PRIORIDAD DE INPUT:

El sistema funciona así:
1. **Si hay input móvil** (joystick movido) ? Usa controles móviles
2. **Si NO hay input móvil** ? Usa teclado/mouse
3. **Ambos pueden funcionar** ? El móvil tiene prioridad

Esto significa que:
- ? En PC puedes usar teclado normalmente
- ? En Android solo se ven los controles táctiles
- ? Puedes probar en PC tocando los botones con el mouse

---

## CONFIGURACIÓN PARA ANDROID BUILD:

### Paso 1: Build Settings
1. File ? Build Settings
2. Platform: Android
3. Switch Platform (esperar a que termine)

### Paso 2: Player Settings
1. En Build Settings, click "Player Settings"
2. **Other Settings:**
   - Package Name: com.TuNombre.EntrelaNeblina
   - Minimum API Level: Android 5.0 'Lollipop' (API level 21)
   - Target API Level: Automatic (highest installed)
   
3. **Resolution and Presentation:**
   - Default Orientation: Landscape Left o Auto Rotation
   - Desmarcar orientaciones que no quieras

### Paso 3: Input System
Asegúrate de que el Input System esté configurado:
1. Edit ? Project Settings ? Player
2. Active Input Handling: Input System Package (New) o Both

---

## TESTING:

### En PC (Editor de Unity):
1. Activa "Desactivar En PC" en PlayerMovementJoystick
2. Los controles móviles se ocultarán automáticamente
3. Usa teclado/mouse normalmente

### En PC (sin desactivar móviles):
1. Desactiva "Desactivar En PC"
2. Los botones se verán en pantalla
3. Puedes hacer click en ellos con el mouse para probar

### En Android:
1. Build ? Build and Run (con dispositivo conectado)
2. Los controles táctiles aparecerán automáticamente
3. Usa el joystick para moverte
4. Toca los botones para saltar/atacar/interactuar

---

## PERSONALIZACIÓN:

### Cambiar Deadzone del Joystick:
```csharp
// En PlayerMovementJoystick, campo:
public float joystickDeadzone = 0.1f; // Más bajo = más sensible
```

### Cambiar Tamaño de Botones:
Selecciona cada botón en el Canvas y ajusta:
- Width: Ancho del botón
- Height: Alto del botón

### Cambiar Posiciones:
Arrastra los botones en el Canvas a donde prefieras.
Recomendaciones:
- Joystick: Inferior izquierda
- Jump: Inferior derecha
- Attack: Al lado del Jump
- Interact: Centro derecha

---

## SOLUCIÓN DE PROBLEMAS:

### El joystick no mueve al personaje:
- ? Verifica que Movement Joystick esté asignado
- ? Revisa que el joystick tenga el componente "FixedJoystick" o "Joystick"
- ? Activa "Mostrar Debug" y mira la consola

### Los botones no funcionan:
- ? Verifica que los botones tengan componente "Button"
- ? Asegúrate de que PlayerMovementJoystick tenga las referencias
- ? NO configures onClick manualmente, el script lo hace

### Los controles se ven en PC:
- ? Marca "Desactivar En PC" en PlayerMovementJoystick
- ? O desactiva manualmente el Canvas en PC

### El personaje no salta:
- ? Verifica que Jump Button esté asignado
- ? Revisa que PlayerMovement esté asignado
- ? Mira la consola con "Mostrar Debug" activado

### Error: "Tag: Interactable is not defined":
**Solución Rápida:**
1. Ve a **Edit ? Project Settings ? Tags and Layers**
2. En la sección **Tags**, click en el **+**
3. Agrega el tag: **Interactable**
4. Asigna este tag a tus NPCs, carteles y objetos interactuables

**O simplemente:**
- Deja el campo **Interact Button** vacío en PlayerMovementJoystick
- El código ya maneja este error con try-catch

### El botón de Interact no hace nada:
- ? Verifica que el tag "Interactable" exista
- ? Asegúrate de que tus objetos interactuables tengan ese tag
- ? Verifica que el rango de interacción sea suficiente (2 unidades por defecto)
- ? Asegúrate de que los objetos tengan un método `OnInteract()`
