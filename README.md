# Sistema de Gestión de Pedidos (GestionPedidosDY)

Este proyecto es una API REST desarrollada con .NET 8 que implementa una arquitectura limpia para gestionar pedidos de productos. La solución integra RabbitMQ para la mensajería asíncrona y MailHog para la captura y visualización de correos electrónicos en un entorno de desarrollo.

## Organización del Código Fuente

El proyecto sigue los principios de la Arquitectura Limpia y está dividido en cuatro capas:

- **`GestionPedidosDY`**: Es el proyecto principal y la capa de presentación (API). Contiene:
  - `Controllers`: Los puntos de entrada de la API.
  - `Program.cs`: La configuración de servicios, dependencias y el pipeline de la aplicación.
  - `OrderProcessingBackgroundService.cs`: El servicio en segundo plano que consume mensajes de RabbitMQ.
  - `appsettings.json`: Configuración de la aplicación.

- **`GestionPedidosDY.Application`**: La capa de aplicación. Contiene la lógica de negocio y orquestación. Aquí se definen:
  - `Contracts`: Las interfaces para los repositorios y servicios externos (`IOrderRepository`, `IEmailService`, `IMessagePublisher`, `IUnitOfWork`).
  - `Dtos`: Objetos de Transferencia de Datos para la comunicación entre capas.
  - `Factories`: Lógica para la creación de objetos complejos.

- **`GestionPedidosDY.Core`**: La capa de dominio. Es el núcleo de la aplicación y contiene las entidades de negocio principales:
  - `Order`, `OrderItem`, `Product`.

- **`GestionPedidosDY.Infraestructure`**: La capa de infraestructura. Contiene las implementaciones de los contratos definidos en la capa de aplicación.
  - `Data`: El `DbContext` de Entity Framework Core y la configuración de la base de datos.
  - `Repositories`: La implementación del patrón Repositorio y Unidad de Trabajo.
  - `Services`: La implementación de servicios externos, como `RabbitMqPublisher` y `MailHogEmailService`.

- **`GestionPedidosDY.Tests`**: Contiene las pruebas unitarias y de integración para la aplicación, utilizando el framework **xUnit**.

## Archivos Docker

El archivo `docker-compose.yml` en la raíz del proyecto se utiliza para orquestar los servicios de infraestructura necesarios para el desarrollo.

### Servicios

1.  **`rabbitmq`**: 
    - **Propósito**: Servidor de mensajería para la comunicación asíncrona.
    - **Puertos**:
      - `5672`: Puerto del protocolo AMQP, utilizado por la aplicación para conectarse.
      - `15672`: Puerto de la interfaz de administración web, accesible en `http://localhost:15672`.

2.  **`mailhog`**:
    - **Propósito**: Servidor de correo electrónico falso que captura los emails enviados por la aplicación.
    - **Puertos**:
      - `1025`: Puerto del servidor SMTP, utilizado por la aplicación para enviar correos.
      - `8025`: Puerto de la interfaz web para visualizar los correos capturados, accesible en `http://localhost:8025`.

## Instrucciones de Ejecución

Sigue estos pasos para ejecutar la solución completa en tu entorno local.

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Pasos

1.  **Levantar Servicios de Infraestructura**:
    Abre una terminal en la raíz del proyecto (donde se encuentra `docker-compose.yml`) y ejecuta el siguiente comando para iniciar RabbitMQ y MailHog en segundo plano:
    ```sh
    docker-compose up -d
    ```

2.  **Ejecutar la Aplicación .NET**:
    Navega al directorio del proyecto principal y ejecuta la aplicación:
    ```sh
    cd GestionPedidosDY
    dotnet run
    ```
    La API se iniciará y estará disponible en `http://localhost:5000`.

3.  **Ejecutar Pruebas**:
    Navega al directorio del proyecto de pruebas y ejecuta los tests:
    ```sh
    cd ..\GestionPedidosDY.Tests
    dotnet test
    ```

4.  **Probar la Funcionalidad**:
    - Abre tu navegador y ve a la interfaz de Swagger UI: `http://localhost:5000/swagger`.
    - Utiliza el endpoint `POST /api/Orders` para crear un nuevo pedido. Envía un cuerpo JSON con los detalles del pedido.
    - Tras crear el pedido, la aplicación publicará un evento en RabbitMQ.
    - El servicio en segundo plano procesará el evento y enviará un correo de confirmación.

5.  **Verificar el Correo**:
    - Para confirmar que el correo se ha enviado y capturado correctamente, abre la interfaz de MailHog en tu navegador: `http://localhost:8025`.
    - Deberías ver el correo de confirmación del pedido en la bandeja de entrada.
