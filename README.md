# ChachaWillka

Tienda ASP.NET Core MVC para licores y cremas artesanales. El cliente arma su cesta, confirma el pedido, envía su evidencia de pago por Yape y consulta cuándo puede recogerlo. El personal verifica el pago y registra el recojo. El administrador gestiona el catálogo, las existencias, los usuarios y los reportes.

## Arranque local

Requiere un SDK compatible con .NET 10 y SQL Server. En Windows puedes usar la instancia LocalDB instalada con Visual Studio. Estos comandos preparan una **base nueva de desarrollo** y crean el primer administrador. Ejecuta todo desde la raíz del repositorio, en la misma ventana de PowerShell.

```powershell
$env:ConnectionStrings__CadenaSQL = 'Server=(localdb)\MSSQLLocalDB;Database=ChachaWillka_Desarrollo;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5'
$credencial = Get-Credential -Message 'Correo y contraseña (al menos 12 caracteres) del administrador local'
$env:Tienda__Admin__Correo = $credencial.UserName
$env:Tienda__Admin__Clave = $credencial.GetNetworkCredential().Password
dotnet run --no-launch-profile -- --inicializar-tienda
Remove-Item Env:Tienda__Admin__Clave
Remove-Item Env:Tienda__Admin__Correo
dotnet run --launch-profile http
```

Abre [la tienda local](http://localhost:5280). El inicializador aplica las migraciones, crea los roles y carga 12 productos con los precios del prototipo. Las existencias iniciales son cero. Registra el stock real desde **Administración → Catálogo e inventario**. Para aplicar la migración a una base anterior, haz un respaldo y pruébala primero en una copia. La migración invalida las antiguas contraseñas en texto plano. Administración puede recuperar cada cuenta histórica con una contraseña nueva después de verificar al titular.

Revisa **Administración → Ajustes de la tienda**. El número de Yape y el horario inicial son provisionales. Las cajas admiten cuatro sabores distintos por defecto. El negocio puede editar los precios, permitir repeticiones, actualizar el horario y registrar el contacto de WhatsApp desde el panel.

## Validación

`dotnet build` comprueba la solución. En una base aislada, el script `tests/flujo_pedidos.py` simula el proceso de compra con cuentas y referencias ficticias. Solo admite una URL local. Usa **una base de pruebas distinta** de la base del negocio, inicia la aplicación y configura estas variables en la sesión donde ejecutarás el script.

```powershell
$env:TIENDA_QA_URL = 'http://127.0.0.1:5280'
$env:TIENDA_QA_ADMIN_CORREO = '<correo del administrador ficticio>'
$env:TIENDA_QA_ADMIN_CLAVE = '<contraseña del administrador ficticio>'
python tests/flujo_pedidos.py
```

## Fotografías

Las fotos de productos provienen del [prototipo de ChachaWillka](https://chachawillkasite.netlify.app/). El logo de perfil proviene del [Instagram de la marca](https://www.instagram.com/chachawillka/). Las dos imágenes referenciales de cajas se generaron a partir de las botellas y el empaque del prototipo y se sirven como WebP.

---

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
