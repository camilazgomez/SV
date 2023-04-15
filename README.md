# SV
Camila Pizarro &
Camila Zumaeta
## Instrucciones
### Para ejecutar este programa es importante primero correr la base de datos. Esta se encuentra en la carpeta SQL, en el archivo llamado *database.sql*. 
Este archivo crea la base de datos con las tablas `RealStateForm`, `People`, `Communes` y `MultiOwner`.
### Para conectarse a la base de datos desde la solución hay que cambiar el nombre del servidor que tiene la conexión a la base de datos, esto se hace en la carpeta raíz (SV) en el archivo *appsettings.json*.
### Con esto es posible compilar el programa. 

## Supuestos
* Cuando no se selecciona el checkbox de porcentaje de derecho no acreditado y tampoco se ingresa un porcentaje de derecho, se asume que el Adquiriente tiene un porcentaje no acreditado.
* Cuando se selecciona el checkbox de porcentaje de derecho no acreditado y se ingresa un porcentaje de derecho, se asume que el Adquiriente tiene un porcentaje no acreditado y el porcentaje ingresado es reemplazado por el porcentaje calculado por el algoritmo.
* Cuando la suma de los porcentajes de derecho de los Adquirientes con porcentaje acreditado es igual a 100, si hay Adquirientes con porcentaje no acreditado se 
les asignará un porcentaje de derecho igual a 0.
