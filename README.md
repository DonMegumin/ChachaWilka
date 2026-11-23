# 📌 Flujo Básico de Git - Proyecto SWAD

## ⚠️ IMPORTANTE

Trabajaremos con un flujo SIMPLE para evitar errores y confusiones.

Usaremos únicamente:

```bash
git status
git pull (antes de empezar a programar y antes de subir cambios al repositorio)
git add .
git commit -m "mensaje"
git push
git log
```

Si alguien tiene dudas o algo raro sucede con Git:

- Avisar al grupo primero
- NO intentar “arreglar” Git por cuenta propia
- NO usar comandos que encuentren en internet sin preguntar

---

# 🚫 PROHIBIDO

## Nunca usar:

```bash
git push --force
```

## Nunca ejecutar:

```bash
git init
```

El repositorio YA está inicializado.

---

# ⚠️ MUY IMPORTANTE - TERMINAL CORRECTA

El proyecto tiene:

```text
system-backend
system-frontend
```

ambos dentro del MISMO repositorio Git.

Por eso, TODOS los comandos Git deben ejecutarse en la RAÍZ del proyecto.

## ✅ Correcto

```text
Proyecto/
│
├── .git
├── system-backend
├── system-frontend
```

La terminal debe estar aquí:

```text
Proyecto/
```

---

## ❌ Incorrecto

NO ejecutar Git dentro de:

```text
system-backend/
```

o

```text
system-frontend/
```

---

# ⚠️ ERROR COMÚN

Como algunos trabajan:

- Backend → IntelliJ / Spring Tool Suite
- Frontend → VS Code

pueden abrir terminales diferentes.

Si ejecutan:

```bash
git status
```

y aparece algo raro como:

```text
not a git repository
```

NO hagan:

```bash
git init
```

Eso puede romper el repositorio o generar conflictos.

Primero verifiquen que estén en la carpeta raíz del proyecto.

---

# ✅ Flujo Correcto de Trabajo

## 1. Abrir terminal en la raíz del proyecto

Deben ver:

```text
.git
system-backend
system-frontend
```

---

## 2. Verificar cambios antes de trabajar

```bash
git status
```

---

## 3. Actualizar el proyecto ANTES de programar

```bash
git pull origin main
```

---

## 4. Programar normalmente

Hacer cambios en frontend o backend.

---

## 5. Revisar cambios realizados

```bash
git status
```

---

## 6. Guardar cambios

```bash
git add .
```

---

## 7. Crear commit

## ✅ Buenos ejemplos

```bash
git commit -m "Agregar login"
git commit -m "Corregir error en ventas"
git commit -m "Mejorar diseño del dashboard"
git commit -m "Agregar CRUD de productos"
```

---

## ❌ Malos ejemplos

```bash
git commit -m "cambios"
git commit -m "update"
git commit -m "aaaa"
git commit -m "avance"
```

---

## 8. Subir cambios

```bash
git push origin main
```

---

## 9. Ver historial de commits

```bash
git log
```

Salir del historial:

```bash
q
```

---

# ⚠️ IMPORTANTE SOBRE EL HISTORIAL

Anteriormente ocurrió un problema donde el historial se perdió por un comando incorrecto.

Por eso:

- NO usar force
- NO hacer reset sin preguntar
- NO modificar historial
- NO usar comandos avanzados si no están seguros

Git nos ayudó a recuperar el proyecto porque detectó diferencias en el historial.

---

# ✅ Configuración Backend Spring Boot

Dentro de:

```text
system-backend/src/main/
```

crear la carpeta:

```text
resources
```

y dentro crear el archivo:

```text
application.properties
```

---

## 📄 application.properties

```properties
spring.application.name=system-backend

spring.datasource.url=jdbc:postgresql://localhost:5433/nombredesubd
spring.datasource.username=
spring.datasource.password=

spring.jpa.database-platform=org.hibernate.dialect.PostgreSQLDialect

spring.jpa.hibernate.ddl-auto=update

spring.jpa.show-sql=true
spring.jpa.properties.hibernate.format_sql=true

server.port=8081
```

---

# ⚠️ IMPORTANTE SOBRE application.properties

El archivo:

```text
application.properties
```

Cada integrante puede configurar:

- su usuario PostgreSQL
- su contraseña
- el nombre de su base de datos

según su computadora.

---

# ✅ Recomendaciones Finales

- Hacer `git pull` antes de empezar a trabajar y antes de subir cambios al repositorio
- Revisar `git status` frecuentemente
- Hacer commits pequeños y claros
- Avisar al grupo si ocurre algo raro
- Mantener la calma si Git muestra errores
- Preguntar antes de ejecutar comandos desconocidos

---

# 📌 Regla Más Importante

Si no entienden un comando:

## NO lo ejecuten.

Primero preguntar en el grupo.
