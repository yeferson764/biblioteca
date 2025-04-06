# biblioteca

# __Configuración inicial__

__1.__ Entrar en el ```appsettings.json``` y en la ```ConnectionDB``` reemplazar los asteriscos que se encuentran en el string para la conexión a la base de datos, o reemplazarla por una a su gusto.

__2.__ Abrir  __Tools__ -> __NuGet Package Manager__ -> __Package Manager Console__.

![image](https://github.com/user-attachments/assets/7d81f878-89ea-4c5b-b016-6ca9b31707ac)

__3.__ Una vez abierta la consola ejecutar los siguientes comandos.
```
add-migration NombreDeLaMigración
```
```
update-database
```

Si no lanza ningun tipo de error, se puede lanzar la aplicación.

![image](https://github.com/user-attachments/assets/01d889b7-4b37-4d1a-9fb2-91dc506966a3)
