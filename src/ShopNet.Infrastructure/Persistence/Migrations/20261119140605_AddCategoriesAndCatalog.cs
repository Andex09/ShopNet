using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNet.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Categories, product description and active flag, and a technology catalog of 62 products. The catalog is content
    /// (admins edit it), so it is inserted with SQL here instead of HasData, which would fight later edits.
    /// </summary>
    public partial class AddCategoriesAndCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            // Fixed ids so the catalog below (and docs) can refer to them.
            migrationBuilder.Sql(
                """
                SET IDENTITY_INSERT Categories ON;
                INSERT INTO Categories (Id, Name) VALUES
                    (1, N'Audio'),
                    (2, N'Periféricos'),
                    (3, N'Portátiles'),
                    (4, N'Monitores'),
                    (5, N'Celulares y tablets'),
                    (6, N'Hogar inteligente'),
                    (7, N'Almacenamiento y redes'),
                    (8, N'Accesorios');
                SET IDENTITY_INSERT Categories OFF;
                """);

            // Existing products land in "Accesorios" and stay active; the 2022 seed rows are updated below.
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // The 2022 seed: products 2 and 3 become real catalog items (same ids, so their orders stay valid).
            // "Platzi" was never a product: deleted when nobody ordered it, deactivated otherwise.
            migrationBuilder.Sql(
                """
                UPDATE Products SET
                    Name = N'Mouse inalámbrico ergonómico multidispositivo',
                    Description = N'Mouse inalámbrico de forma ergonómica con sensor de 8.000 DPI que funciona incluso sobre vidrio, rueda de desplazamiento electromagnética ultrarrápida y botones silenciosos. Se conecta hasta a tres equipos por Bluetooth o receptor USB y cambia entre ellos con un botón. Batería recargable por USB-C con hasta 70 días de uso.',
                    Price = 350000, CategoryId = 2
                WHERE Id = 2;

                UPDATE Products SET
                    Name = N'Audífonos de diadema con cable USB para oficina',
                    Description = N'Audífonos de diadema livianos con micrófono con cancelación de ruido, control de volumen en el cable y conexión USB plug and play. Almohadillas acolchadas para usar durante toda la jornada en videollamadas, clases virtuales y centros de contacto.',
                    Price = 200000, CategoryId = 1
                WHERE Id = 3;

                IF EXISTS (SELECT 1 FROM Orders WHERE ProductId = 1)
                    UPDATE Products SET IsActive = 0 WHERE Id = 1 AND Name = N'Platzi';
                ELSE
                    DELETE FROM Products WHERE Id = 1 AND Name = N'Platzi';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO Products (Name, Description, Price, CategoryId, IsActive) VALUES
                -- 1 Audio
                (N'Audífonos inalámbricos con cancelación de ruido', N'Audífonos de diadema Bluetooth 5.3 con cancelación activa de ruido, modo de sonido ambiente y hasta 30 horas de batería. Carga rápida por USB-C: 10 minutos dan 5 horas de música. Micrófonos para llamadas claras. Ideales para viajar, trabajar en espacios abiertos o escuchar música sin distracciones.', 649900, 1, 1),
                (N'Audífonos True Wireless deportivos', N'Audífonos totalmente inalámbricos con ganchos que no se caen al correr, resistencia al sudor y al agua IPX5 y estuche de carga con 24 horas de autonomía total. Controles táctiles para música y llamadas. Pensados para el gimnasio, el ciclismo y el entrenamiento al aire libre.', 289900, 1, 1),
                (N'Parlante Bluetooth portátil resistente al agua', N'Parlante portátil con sonido de 360 grados y bajos profundos, resistente al agua y al polvo IP67: aguanta la piscina, la playa y la lluvia. 12 horas de batería, correa para colgarlo y modo para emparejar dos parlantes en estéreo.', 249900, 1, 1),
                (N'Barra de sonido 2.1 con subwoofer inalámbrico', N'Barra de sonido para televisor con subwoofer inalámbrico, conexión HDMI ARC y óptica, Bluetooth para música desde el celular y modo de diálogo que realza las voces en películas y series. Se controla con el control remoto del televisor.', 899900, 1, 1),
                (N'Micrófono USB de condensador para streaming', N'Micrófono de condensador USB con patrón cardioide que capta tu voz y reduce el ruido de la habitación. Salida de audífonos para monitoreo sin retraso, botón de silencio y brazo articulado. Ideal para podcasts, streaming, grabar clases y reuniones virtuales.', 329900, 1, 1),
                (N'Audífonos gamer con sonido envolvente 7.1', N'Audífonos para videojuegos con sonido envolvente virtual 7.1 para ubicar pasos y disparos, micrófono desmontable con cancelación de ruido y almohadillas de espuma viscoelástica. Compatibles con PC, consolas y celulares.', 279900, 1, 1),
                (N'Receptor de audio Bluetooth para equipos de sonido', N'Pequeño receptor que convierte un equipo de sonido, parlantes o un carro con entrada auxiliar en un sistema inalámbrico. Conecta el celular por Bluetooth y escucha tu música en el equipo de siempre. Salida RCA y de 3,5 mm.', 89900, 1, 1),
                -- 2 Periféricos
                (N'Teclado mecánico inalámbrico compacto 75%', N'Teclado mecánico de formato 75% con switches táctiles intercambiables, retroiluminación blanca y conexión Bluetooth, receptor 2,4 GHz o cable USB-C. Compatible con Windows y Mac. Ocupa poco espacio en el escritorio y deja más lugar para el mouse.', 459900, 2, 1),
                (N'Combo teclado y mouse inalámbricos silenciosos', N'Teclado de tamaño completo con teclas silenciosas y mouse inalámbrico de clic suave que comparten un solo receptor USB. Las pilas duran hasta 24 meses. Una opción económica para la oficina, el estudio o la casa.', 159900, 2, 1),
                (N'Mouse gamer RGB de 26.000 DPI', N'Mouse para videojuegos ultraliviano de 60 gramos con sensor óptico de 26.000 DPI, seis botones programables e iluminación RGB personalizable. Cable flexible que no se enreda y pies de PTFE para un deslizamiento suave.', 219900, 2, 1),
                (N'Teclado ergonómico dividido', N'Teclado dividido en dos mitades con inclinación ajustable y reposamuñecas acolchado que reduce la tensión en muñecas, hombros y antebrazos. Recomendado para quienes escriben muchas horas al día o sienten molestias al usar un teclado tradicional.', 389900, 2, 1),
                (N'Cámara web Full HD 1080p con micrófono dual', N'Cámara web Full HD a 30 cuadros por segundo con autoenfoque, corrección automática de luz y dos micrófonos estéreo. Tapa de privacidad incorporada y clip que se ajusta a monitores y portátiles. Para videollamadas, clases y entrevistas de trabajo.', 249900, 2, 1),
                (N'Tableta digitalizadora para dibujo', N'Tableta gráfica con lápiz sin batería de 8.192 niveles de presión, área activa de 10 x 6 pulgadas y ocho teclas de acceso rápido. Para ilustración digital, edición de fotos, diseño, firmas electrónicas y escribir a mano en clases virtuales.', 299900, 2, 1),
                (N'Mousepad XL con base antideslizante', N'Tapete de escritorio de 90 x 40 cm que cubre teclado y mouse, con superficie de tela de deslizamiento uniforme, bordes cosidos que no se deshilachan y base de caucho antideslizante.', 69900, 2, 1),
                -- 3 Portátiles
                (N'Portátil ultradelgado 14" para trabajo y estudio', N'Portátil de 14 pulgadas y 1,3 kg con procesador Intel Core Ultra 5, 16 GB de RAM y SSD de 512 GB. Hasta 15 horas de batería, cámara Full HD y lector de huella. Para trabajar, estudiar y viajar con un equipo liviano y rápido.', 3899900, 3, 1),
                (N'Portátil gamer 15,6" con RTX 4060', N'Portátil para videojuegos con procesador AMD Ryzen 7, tarjeta gráfica NVIDIA GeForce RTX 4060, 16 GB de RAM, SSD de 1 TB y pantalla de 144 Hz. Refrigeración con doble ventilador y teclado RGB. También rinde en edición de video y modelado 3D.', 6499900, 3, 1),
                (N'Portátil 2 en 1 con pantalla táctil y lápiz', N'Portátil convertible de 13,3 pulgadas que gira 360 grados para usarlo como tableta, con pantalla táctil, lápiz incluido, 8 GB de RAM y SSD de 256 GB. Cómodo para tomar notas a mano, firmar documentos y presentar.', 3299900, 3, 1),
                (N'Portátil básico 15,6" para el hogar', N'Portátil de 15,6 pulgadas con Intel Core i3, 8 GB de RAM y SSD de 256 GB. Suficiente para navegar, ver series, hacer tareas, clases virtuales y trabajo de oficina. Teclado numérico completo.', 1899900, 3, 1),
                (N'Portátil para creadores 16" con pantalla OLED', N'Portátil de 16 pulgadas con pantalla OLED 3,2K de color preciso (100% DCI-P3), 32 GB de RAM, NVIDIA RTX 4070 y SSD de 1 TB. Pensado para edición de video 4K, fotografía, diseño y producción musical.', 9499900, 3, 1),
                (N'Chromebook 11,6" para estudiantes', N'Portátil compacto con ChromeOS, resistente a caídas desde escritorio y con teclado antiderrames. Arranca en segundos, se actualiza solo y su batería dura un día completo de clases.', 1199900, 3, 1),
                (N'Base refrigerante para portátil con 5 ventiladores', N'Base con cinco ventiladores silenciosos que bajan la temperatura del portátil, seis alturas para mejorar la postura y dos puertos USB. Para portátiles de hasta 17 pulgadas que se calientan al jugar o trabajar.', 129900, 3, 1),
                -- 4 Monitores
                (N'Monitor 27" 4K IPS para diseño', N'Monitor de 27 pulgadas con resolución 4K, panel IPS con 99% de sRGB y HDR400. Un solo cable USB-C transmite video, datos y carga el portátil a 65 W. Para diseño gráfico, fotografía y programación con texto nítido.', 1899900, 4, 1),
                (N'Monitor gamer 27" QHD de 165 Hz', N'Monitor de 27 pulgadas QHD con frecuencia de 165 Hz y 1 ms de respuesta para juegos fluidos y sin desgarros, compatible con FreeSync y G-Sync. Base regulable en altura e inclinación.', 1499900, 4, 1),
                (N'Monitor 24" Full HD para oficina', N'Monitor de 24 pulgadas Full HD con panel IPS, filtro de luz azul y tecnología sin parpadeo para cuidar la vista en jornadas largas. Bordes delgados para armar configuraciones de dos pantallas y compatible con soportes VESA.', 599900, 4, 1),
                (N'Monitor ultrawide 34" curvo', N'Monitor curvo de 34 pulgadas en formato 21:9 que equivale a dos pantallas sin el borde del medio. Ideal para multitarea, hojas de cálculo, programación y edición en línea de tiempo. Incluye conmutador KVM para dos equipos.', 2299900, 4, 1),
                (N'Monitor portátil 15,6" USB-C', N'Segunda pantalla portátil de 15,6 pulgadas y menos de 1 kg que se conecta y alimenta con un solo cable USB-C. Funda que sirve de soporte. Para trabajar con dos pantallas en cualquier parte o conectar una consola en viajes.', 799900, 4, 1),
                (N'Soporte de brazo para dos monitores', N'Brazo articulado con pistón de gas para dos monitores de hasta 32 pulgadas. Libera espacio en el escritorio, permite girar las pantallas en vertical y organiza los cables por dentro del brazo.', 289900, 4, 1),
                (N'Barra de luz para monitor', N'Lámpara que se apoya sobre el monitor e ilumina el escritorio sin reflejos en la pantalla. Brillo y temperatura de color ajustables, alimentación por USB. Reduce la fatiga visual al trabajar de noche.', 159900, 4, 1),
                -- 5 Celulares y tablets
                (N'Smartphone gama media 6,6" AMOLED 128 GB', N'Celular 5G con pantalla AMOLED de 6,6 pulgadas a 120 Hz, cámara principal de 50 MP con estabilización, 128 GB de almacenamiento y batería de 5.000 mAh con carga rápida de 33 W. Buen equilibrio entre precio y rendimiento.', 1299900, 5, 1),
                (N'Smartphone gama alta 6,7" 256 GB', N'Celular de gama alta con pantalla de 6,7 pulgadas, triple cámara con zoom óptico de 3x, grabación de video 4K, resistencia al agua IP68 y 256 GB. Carga inalámbrica y cinco años de actualizaciones.', 4299900, 5, 1),
                (N'Smartphone básico 6,5" 64 GB', N'Celular sencillo con pantalla grande de 6,5 pulgadas, batería que dura dos días y 64 GB de almacenamiento. Ideal como primer celular, para adultos mayores o como equipo de respaldo.', 499900, 5, 1),
                (N'Tablet 11" 128 GB con lápiz', N'Tablet de 11 pulgadas con pantalla 2,5K, cuatro parlantes, 128 GB y lápiz incluido. Para ver series, leer, tomar apuntes a mano, dibujar y asistir a clases virtuales.', 1599900, 5, 1),
                (N'Tablet infantil 8" con control parental', N'Tablet de 8 pulgadas para niños con funda antigolpes, control parental, límites de tiempo de uso y contenido educativo preinstalado.', 449900, 5, 1),
                (N'Lector de libros electrónicos 6"', N'Lector con pantalla de tinta electrónica de 6 pulgadas que se lee como papel, sin reflejos ni con sol directo. Luz cálida ajustable para leer de noche, resistente al agua y batería que dura semanas.', 599900, 5, 1),
                (N'Reloj inteligente con GPS y monitoreo de salud', N'Reloj inteligente con GPS integrado, medición de ritmo cardíaco, oxígeno en sangre y calidad del sueño, más de 100 modos deportivos y notificaciones del celular. Hasta 10 días de batería y resistente al agua para nadar.', 749900, 5, 1),
                (N'Pulsera de actividad', N'Pulsera liviana que cuenta pasos y calorías, registra el sueño y muestra notificaciones. Batería de 14 días y resistente al agua. Para empezar a llevar una vida más activa.', 159900, 5, 1),
                -- 6 Hogar inteligente
                (N'Parlante inteligente con asistente de voz', N'Parlante con asistente de voz para poner música, preguntar el clima, crear alarmas y recordatorios y controlar bombillos, enchufes y cámaras inteligentes de la casa sin tocar el celular.', 249900, 6, 1),
                (N'Bombillo LED inteligente Wi-Fi RGB', N'Bombillo LED Wi-Fi de 16 millones de colores y blancos regulables que se controla desde el celular o por voz. Programa horarios para encender y apagar, y crea ambientes para leer, ver películas o dormir. No necesita hub.', 49900, 6, 1),
                (N'Enchufe inteligente Wi-Fi con medición de consumo', N'Enchufe Wi-Fi que enciende y apaga cualquier aparato desde el celular, con horarios programados y medición del consumo de energía para saber cuánto gasta cada electrodoméstico.', 59900, 6, 1),
                (N'Cámara de seguridad Wi-Fi para interiores 2K', N'Cámara 2K que gira 360 grados, con visión nocturna, detección de personas y movimiento, alertas en el celular y audio bidireccional para hablar con quien esté en casa. Graba en microSD o en la nube. Útil para cuidar mascotas, niños o adultos mayores.', 189900, 6, 1),
                (N'Cámara de seguridad exterior con panel solar', N'Cámara inalámbrica para exteriores con batería y panel solar que la mantiene cargada, resistencia a lluvia IP65, visión nocturna a color y alertas en el celular. Se instala sin cables en minutos.', 549900, 6, 1),
                (N'Timbre inteligente con video', N'Timbre con cámara que muestra en el celular quién llega a la puerta y permite hablar con la persona aunque no estés en casa. Detección de movimiento y grabación de visitas.', 399900, 6, 1),
                (N'Cerradura inteligente con huella digital', N'Cerradura que abre con huella digital, clave numérica, tarjeta, llave o desde la aplicación. Claves temporales para visitas y registro de quién entró y a qué hora.', 899900, 6, 1),
                (N'Aspiradora robot con mapeo láser', N'Aspiradora robot que mapea la casa con láser, aspira y trapea, evita escaleras y vuelve sola a cargar. Programa horarios y zonas prohibidas desde la aplicación. Ideal para hogares con mascotas.', 1499900, 6, 1),
                -- 7 Almacenamiento y redes
                (N'Disco SSD externo portátil 1 TB USB-C', N'Disco de estado sólido externo de 1 TB con velocidades de hasta 1.050 MB/s, resistente a caídas y del tamaño de una tarjeta. Para respaldar fotos y videos, llevar proyectos pesados o ampliar el espacio de una consola.', 449900, 7, 1),
                (N'Disco duro externo 2 TB', N'Disco duro externo USB 3.0 de 2 TB para copias de seguridad económicas de documentos, fotos y películas. Funciona en Windows y Mac sin instalar nada.', 329900, 7, 1),
                (N'Memoria USB 128 GB metálica', N'Memoria USB 3.2 de 128 GB con carcasa metálica resistente y argolla para el llavero. Para llevar documentos, presentaciones y archivos entre equipos.', 59900, 7, 1),
                (N'Tarjeta microSD 256 GB', N'Tarjeta microSD de 256 GB y alta velocidad para grabar video 4K en cámaras de acción y drones, y ampliar el almacenamiento de celulares, tablets y consolas portátiles. Incluye adaptador SD.', 99900, 7, 1),
                (N'SSD NVMe M.2 interno 1 TB', N'Unidad de estado sólido NVMe M.2 de 1 TB con velocidades de hasta 7.000 MB/s. La mejora más notoria para que un computador o portátil lento arranque y abra programas en segundos.', 389900, 7, 1),
                (N'Router Wi-Fi 6 doble banda', N'Router Wi-Fi 6 AX3000 de doble banda con cobertura para toda la casa y capacidad para muchos dispositivos conectados a la vez sin perder velocidad. Control parental y red de invitados desde la aplicación.', 349900, 7, 1),
                (N'Sistema Wi-Fi mesh de 3 nodos', N'Tres equipos que forman una sola red Wi-Fi y eliminan las zonas sin señal en casas grandes o de varios pisos. Te mueves por la casa sin cortes en videollamadas ni series.', 899900, 7, 1),
                (N'Repetidor Wi-Fi de enchufe', N'Repetidor que se conecta a un enchufe y amplía la señal Wi-Fi hasta habitaciones lejanas, el patio o el segundo piso. Se configura en minutos con un botón.', 99900, 7, 1),
                -- 8 Accesorios
                (N'Cargador USB-C GaN de 65 W con 3 puertos', N'Cargador compacto de tecnología GaN con dos puertos USB-C y uno USB-A que carga portátil, celular y tablet al mismo tiempo. Del tamaño de un cargador de celular, ideal para viajar con un solo cargador.', 179900, 8, 1),
                (N'Batería externa 20.000 mAh carga rápida', N'Batería portátil de 20.000 mAh que carga un celular entre tres y cuatro veces, con carga rápida de 22,5 W, pantalla de porcentaje y dos dispositivos a la vez. Para viajes, días largos y emergencias.', 139900, 8, 1),
                (N'Base de carga inalámbrica 3 en 1', N'Base que carga de forma inalámbrica el celular, el reloj inteligente y los audífonos al mismo tiempo. Deja el escritorio o la mesa de noche sin cables.', 199900, 8, 1),
                (N'Hub USB-C 7 en 1', N'Adaptador USB-C con salida HDMI 4K, tres puertos USB-A, lector de tarjetas SD y microSD, puerto Ethernet y carga de 100 W. Convierte un portátil delgado en un puesto de trabajo completo.', 189900, 8, 1),
                (N'Mochila antirrobo para portátil 15,6"', N'Mochila con cremalleras ocultas, bolsillo trasero secreto, tela repelente al agua, compartimiento acolchado para portátil de 15,6 pulgadas y puerto USB para cargar el celular con una batería externa.', 229900, 8, 1),
                (N'Soporte elevador de aluminio para portátil', N'Soporte plegable que eleva la pantalla del portátil a la altura de los ojos para mejorar la postura y ventilar el equipo. Liviano para llevarlo a la oficina o la universidad.', 99900, 8, 1),
                (N'Kit de limpieza para pantallas y teclados', N'Spray limpiador seguro para pantallas, paño de microfibra y cepillo para teclados. Quita huellas, polvo y grasa de celulares, portátiles, monitores y televisores.', 39900, 8, 1),
                (N'Cable HDMI 2.1 de 2 metros 8K', N'Cable HDMI 2.1 de 48 Gbps compatible con 8K a 60 Hz y 4K a 120 Hz para consolas de última generación, televisores y monitores gamer.', 59900, 8, 1);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Removes categories, descriptions and the active flag. The catalog rows stay (they are valid products and may
        /// have orders); "Platzi" is not restored.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
