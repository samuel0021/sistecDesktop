# Git - Guia de Referência para Projetos WPF

## 10 Comandos Essenciais

Estes são os comandos que você vai usar no dia a dia:

1. `git status`  
   Ver o status do projeto (alterações, commits pendentes)

2. `git add .`  
   Adicionar todas as mudanças ao staging

3. `git commit -m "Mensagem"`  
   Criar commit com mensagem

4. `git push`  
   Enviar commits locais para o GitHub

5. `git pull`  
   Trazer alterações do GitHub para o projeto local

6. `git branch`  
   Listar branches locais

7. `git checkout nome-do-branch`  
   Mudar para outro branch

8. `git checkout -b nome-do-branch`  
   Criar e mudar para um novo branch

9. `git log --oneline`  
   Ver histórico resumido de commits

10. `git fetch`  
    Buscar alterações do remoto sem alterar o projeto local

**Fluxo diário sugerido:**
```bash
git pull
git add .
git commit -m "Mensagem"
git push
