# EnemyController

Componente para controle de inimigos com múltiplos tipos de movimento e comportamento configuráveis via Inspector.

---

## Features

- Diferentes tipos de movimento: estático, patrulha, perseguição, fuga e caminho personalizado.
- Movimentação ativada por diferentes gatilhos: sempre, proximidade do jogador ou eventos externos.
- Configurações de combate: vida, dano, colisão e interação com o jogador (ex: matar ao contato).
- Flip automático do sprite com opção de usar escala ou flipX.
- Eventos customizáveis no início/fim do movimento e detecção do jogador.
- Controle de colisão para ignorar colisão enquanto jogador está rolando.

---

## Inspector Variables

### Combat Settings  
- **isDestructible (bool):** Pode receber dano e ser destruído.  
- **damageCooldown (float):** Tempo entre danos consecutivos.  
- **health (int):** Vida do inimigo.  
- **killsPlayerOnContact (bool):** Mata o jogador no contato.  
- **ignoreRollingPlayer (bool):** Ignora colisão se jogador estiver rolando.  

### Movement Settings  
- **movementType (enum):** Tipo de movimento (`Static`, `Patrol`, `ChasePlayer`, `FleePlayer`, `CustomPath`).  
- **movementTrigger (enum):** Quando o movimento é ativado (`Always`, `OnPlayerProximity`, `OnEvent`).  
- **moveSpeed (float):** Velocidade de movimento.  
- **movementDirection (Vector2):** Direção inicial para patrulha/caminho.  
- **patrolDistance (float):** Distância do percurso de patrulha.  
- **playerDetectionRange (float):** Raio para detectar jogador (usado em `OnPlayerProximity`).  
- **movementCooldown (float):** Tempo de espera entre mudanças (ex: patrulha).  
- **customPathPoints (Transform[]):** Pontos para caminho personalizado.  

### Visual Settings  
- **flipSpriteBasedOnPlayer (bool):** Vira sprite para olhar para o jogador.  
- **flipUsingScale (bool):** Usa escala para virar sprite ao invés do `flipX`.  
- **flipThreshold (float):** Distância mínima para virar o sprite (evita oscilações).  
- **spriteRenderer (SpriteRenderer):** Referência ao componente de sprite do inimigo.  

### Collision Settings  
- **enemyCollider (Collider2D):** Collider do inimigo para controle manual.  
- **restoreColliderAfterRoll (bool):** Reativa o collider após o jogador parar de rolar.  

### Events  
- **onMovementStart (UnityEvent):** Evento disparado quando o movimento começa.  
- **onMovementEnd (UnityEvent):** Evento disparado quando o movimento termina.  
- **onPlayerDetected (UnityEvent):** Evento disparado quando o jogador entra no raio de detecção.  

---

## Notes  
- Variáveis como `playerDetectionRange` e `patrolDistance` só funcionam se o tipo de movimento ou trigger correspondente estiver selecionado.  
- Eventos permitem vincular ações no Inspector, como tocar sons ou spawnar efeitos.  
- `customPathPoints` deve receber objetos "Waypoint" para definir o caminho.

