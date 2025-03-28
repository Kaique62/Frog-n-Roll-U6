Vou explicar **todas as variáveis visíveis no Inspector** (marcadas com `[Header]`), organizadas por seção:

---

### **Combat Settings** *(Configurações de Combate)*
| Variável | Tipo | Descrição |
|----------|------|-----------|
| **isDestructible** | `bool` | Se `true`, o inimigo pode ser destruído ao receber dano. |
| **damageCooldown** | `float` | Tempo (em segundos) entre um dano e outro que o inimigo pode receber. |
| **health** | `int` | Quantidade de vida do inimigo. Quando chegar a 0, ele será destruído. |
| **killsPlayerOnContact** | `bool` | Se `true`, o jogador morre imediatamente ao tocar no inimigo. |
| **ignoreRollingPlayer** | `bool` | Se `true`, o inimigo ignora colisões com o jogador quando ele está rolando. |

---

### **Movement Settings** *(Configurações de Movimento)*
| Variável | Tipo | Descrição |
|----------|------|-----------|
| **movementType** | `enum MovementType` | **Tipo de movimento:**<br>- `Static`: Não se move.<br>- `Patrol`: Movimento de vai e vem.<br>- `ChasePlayer`: Persegue o jogador.<br>- `FleePlayer`: Foge do jogador.<br>- `CustomPath`: Segue um caminho pré-definido. |
| **movementTrigger** | `enum MovementTrigger` | **Quando o movimento é ativado:**<br>- `Always`: Movimenta-se constantemente.<br>- `OnPlayerProximity`: Só se move quando o jogador está próximo.<br>- `OnEvent`: Movimento controlado por eventos externos. |
| **moveSpeed** | `float` | Velocidade de movimento do inimigo. |
| **movementDirection** | `Vector2` | Direção inicial do movimento (usado em `Patrol` e `CustomPath`). |
| **patrolDistance** | `float` | Distância máxima que o inimigo percorre no modo `Patrol`. |
| **playerDetectionRange** | `float` | Raio de detecção do jogador (usado com `OnPlayerProximity`). |
| **movementCooldown** | `float` | Tempo de espera entre mudanças de direção (ex: em `Patrol`). |
| **customPathPoints** | `Transform[]` | Pontos do caminho personalizado (usado com `CustomPath`). |

---

### **Visual Settings** *(Configurações Visuais)*
| Variável | Tipo | Descrição |
|----------|------|-----------|
| **flipSpriteBasedOnPlayer** | `bool` | Se `true`, o sprite vira para a direção do jogador. |
| **flipUsingScale** | `bool` | Se `true`, usa a escala do objeto para virar o sprite (ao invés de `flipX`). |
| **flipThreshold** | `float` | Distância mínima para o sprite virar (evita oscilações). |
| **spriteRenderer** | `SpriteRenderer` | Referência ao componente de sprite do inimigo. |

---

### **Collision Settings** *(Configurações de Colisão)*
| Variável | Tipo | Descrição |
|----------|------|-----------|
| **enemyCollider** | `Collider2D` | Collider do inimigo (para controle manual). |
| **restoreColliderAfterRoll** | `bool` | Se `true`, reativa o collider após o jogador parar de rolar. |

---

### **Events** *(Eventos)*
| Variável | Tipo | Descrição |
|----------|------|-----------|
| **onMovementStart** | `UnityEvent` | Disparado quando o movimento começa. |
| **onMovementEnd** | `UnityEvent` | Disparado quando o movimento termina. |
| **onPlayerDetected** | `UnityEvent` | Disparado quando o jogador entra no raio de detecção. |

---

### **Detalhes Adicionais**:
- **Variáveis como `playerDetectionRange` e `patrolDistance** só funcionam se o tipo de movimento ou trigger correspondente estiver selecionado.
- **Os eventos** (ex: `onPlayerDetected`) permitem vincular ações no Inspector, como tocar sons ou spawnar efeitos.
- **CustomPathPoints** requer que você arraste objetos "Waypoint" no Inspector para definir o caminho.