# 🎯 Notification System Setup Guide

## Problem Diagnosis

**Síntomas:**
- Las notificaciones se superponen cuando recoges múltiples items
- Al volver del combate, todas las notificaciones aparecen simultáneamente
- El layout se descontrola con más de 3 notificaciones

**Causas Raíz:**
1. **LayoutElement faltando** → Sin control de tamaño preferido
2. **VerticalLayoutGroup mal configurado** → Force Expand Height activado
3. **Sin espaciado entre spawns** → Todas las animaciones comienzan al mismo tiempo
4. **ContentSizeFitter no optimizado** → No reajusta el container

---

## ✅ Guía de Configuración Correcta

### Paso 1: Estructura del Prefab NotificationSlot

```
NotificationSlot (GameObject)
├── RectTransform
│   └── Anchor: Top-Center (anchorMin: 0.5, 1 | anchorMax: 0.5, 1)
│   └── SizeDelta: (400, 80) [ajusta según tu diseño]
│
├── LayoutElement ⭐ CRÍTICO
│   ├── Preferred Width: 400
│   ├── Preferred Height: 80
│   └── Layout Priority: 1 [MÁXIMA PRIORIDAD]
│
├── CanvasGroup [ya está en el script]
│
├── Image (Background) - para el color de fondo
├── Image (TypeBadge) - para el icono tipo
├── Image (ItemIcon) - para el ícono del item
├── TextGroup (RectTransform)
│   ├── TitleText (TMP_Text) - "¡Botín!" / "¡Recogiste!"
│   └── NameText (TMP_Text) - nombre + cantidad
└── Image (Glow, opcional)
```

### Paso 2: Configuración del Container SlotsContainer

```
SlotsContainer (RectTransform)
├── RectTransform:
│   ├── Anchor: Top-Left o Top-Right (según tu pantalla)
│   ├── Position: esquina donde quieres las notificaciones
│   ├── SizeDelta: (420, 500) [suficiente para 3+ notificaciones]
│
├── Vertical Layout Group ⭐ CRÍTICO
│   ├── Padding: (10, 10, 10, 10) [opcional]
│   ├── Spacing: 10 [espacio entre notificaciones]
│   ├── Child Force Expand Height: FALSE ✅ [NO expandir]
│   ├── Child Force Expand Width: FALSE [opcional]
│   ├── Child Control Height: TRUE ✅ [respetar preferencias]
│   ├── Child Control Width: FALSE o TRUE
│   ├── Child Scale Width: FALSE
│   ├── Child Scale Height: FALSE
│   └── Child Alignment: Upper Left
│
├── ContentSizeFitter
│   ├── Horizontal Fit: Preferred Size (o Unconstrained)
│   ├── Vertical Fit: Preferred Size ✅ [reajusta altura automáticamente]
│
└── Canvas Group (opcional, para fade del container completo)
```

### Paso 3: Asignación de Referencias en Inspector

En `ItemNotificationUI.cs`:
- **Slot Prefab**: Arrastra el prefab NotificationSlot aquí
- **Slots Container**: Arrastra el GameObject SlotsContainer
- **Display Duration**: 3.0 segundos
- **Anim In Duration**: 0.3 segundos
- **Anim Out Duration**: 0.4 segundos
- **Delay Between Spawns**: 0.05 segundos ⭐ IMPORTANTE

### Paso 4: Ajustar en NotificationSlotUI Inspector

- **Preferred Height**: 80 (debe coincidir con RectTransform SizeDelta.y)
- **Preferred Width**: 400 (debe coincidir con RectTransform SizeDelta.x)
- **Slide Offset**: 80 (píxeles de deslizamiento)

---

## 🔍 Troubleshooting

### ❌ Las notificaciones aún se superponen

**Solución:**
1. Verifica que `LayoutElement` existe en el prefab NotificationSlot
2. Confirma que **Child Force Expand Height = FALSE** en VerticalLayoutGroup
3. Aumenta el **Spacing** en VerticalLayoutGroup (10-15px)
4. Asegúrate de que **Content Size Fitter** tiene **Vertical Fit = Preferred Size**

### ❌ Las notificaciones desaparecen demasiado rápido

**Solución:**
- Aumenta **Display Duration** en ItemNotificationUI (3.0 → 4.0 o 5.0)

### ❌ La cola se comporta extrañamente

**Solución:**
1. Abre la consola y busca logs de `[ItemNotificationUI]`
2. Verifica que **Delay Between Spawns** sea > 0 (recomendado: 0.05-0.1)
3. Comprueba que **Max Simultaneous** es razonable (3-5)

### ❌ Las notificaciones se ven cortadas

**Solución:**
- Aumenta **SizeDelta** del SlotsContainer en el eje Y
- Comprueba los límites del Canvas y la posición del container

---

## 📊 Ejemplo de Valores Recomendados

```csharp
// ItemNotificationUI
displayDuration = 3.0f;
animInDuration = 0.3f;
animOutDuration = 0.4f;
delayBetweenSpawns = 0.05f;  // Crucial para evitar overlap
maxSimultaneous = 3;

// NotificationSlotUI
preferredHeight = 80f;
preferredWidth = 400f;
slideOffset = 80f;
```

---

## 🎬 Flujo Esperado

```
1. Recoges Item A → ShowNotification(A)
   ├─ activeSlots.Count = 0 < maxSimultaneous
   ├─ SpawnSlot(A)
   └─ AnimateIn (0.3s) + Wait (3.0s) + AnimateOut (0.4s) = 3.7s total

2. [0.05s después] Recoges Item B → ShowNotification(B)
   ├─ activeSlots.Count = 1 < maxSimultaneous
   ├─ SpawnSlot(B)
   └─ Layout recalcula → B se posiciona debajo de A

3. [0.05s después] Recoges Item C → ShowNotification(C)
   ├─ activeSlots.Count = 2 < maxSimultaneous
   ├─ SpawnSlot(C)
   └─ Layout recalcula → C se posiciona debajo de B

4. [3.7s después] A finishes → Destroy(A.gameObject)
   ├─ activeSlots.Remove(A)
   ├─ Layout recalcula → B y C suben
   ├─ Si hay cola, SpawnSlot(nextInQueue)
   └─ ✅ Sin overlap

5. Vuelves del combate → itemNotificationManager.FlushPendingQueue()
   ├─ 10 items en cola
   ├─ Spawn 1 cada 0.05s = suave progresión
   └─ ✅ No caos visual
```

---

## 🛠️ Debug Mode

Agrega esto en `ItemNotificationUI.cs` para más logs:

```csharp
private void SpawnSlot(ItemNotificationManager.NotificationData data)
{
    Debug.Log($"[ItemNotificationUI] Spawning: {data.itemName} x{data.amount}");
    Debug.Log($"  Active Slots: {activeSlots.Count}/{maxSimultaneous}");
    Debug.Log($"  Pending Queue: {displayQueue.Count}");
    // ... rest of method
}

private IEnumerator SlotLifetime(NotificationSlotUI slot)
{
    Debug.Log($"[ItemNotificationUI] Slot lifetime started: {slot.name}");
    yield return StartCoroutine(slot.AnimateIn(animInDuration));
    yield return new WaitForSeconds(displayDuration);
    yield return StartCoroutine(slot.AnimateOut(animOutDuration));
    Debug.Log($"[ItemNotificationUI] Slot destroyed");
    // ... rest of method
}
```

---

## 📌 Checklist Final

- [ ] NotificationSlot prefab tiene **LayoutElement**
- [ ] LayoutElement: **Preferred Height = 80**, **Layout Priority = 1**
- [ ] SlotsContainer: **Child Force Expand Height = FALSE**
- [ ] SlotsContainer: **Child Control Height = TRUE**
- [ ] ContentSizeFitter: **Vertical Fit = Preferred Size**
- [ ] ItemNotificationUI: **Delay Between Spawns > 0**
- [ ] ItemNotificationUI: **Max Simultaneous = 3-5**
- [ ] Prefab References asignadas en Inspector
- [ ] Display Duration >= 3.0 segundos
- [ ] Prueba con 10+ items simultáneos ✅

---

**¿Aún hay problemas?** Revisa los logs de consola y compara tu setup con esta guía punto por punto.
