using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    protected GameObject objectToSpawn;
    [SerializeField]
    protected int poolSize;
    protected int currentSize;
    protected Queue<GameObject> objectPool;


    private void Awake()
    {
        objectPool = new Queue<GameObject>();

        // Inicializamos los objetos del pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            obj.SetActive(false); // Los objetos comienzan desactivados
            objectPool.Enqueue(obj); // Se agregan al pool
        }
    }

    public virtual GameObject SpawnObject(GameObject currentObject = null)
    {
        if (currentObject == null)
            currentObject = objectToSpawn;

        GameObject spawnedObject = null;

        if (objectPool.Count > 0)
        {
            // Sacamos el objeto del pool
            spawnedObject = objectPool.Dequeue();
            spawnedObject.SetActive(true); // Activamos el objeto para usarlo

            // Resetear el objeto (transformación, etc.)
            spawnedObject.transform.position = transform.position;
            spawnedObject.transform.rotation = Quaternion.identity;
        }
        else
        {
            // Si no hay objetos en el pool, instanciamos un nuevo objeto
            spawnedObject = Instantiate(currentObject, transform.position, Quaternion.identity);
            currentSize++;
        }

        return spawnedObject;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);  // Desactiva el objeto
        obj.transform.position = Vector3.zero;  // Opcional: Restablece la posición
        obj.transform.rotation = Quaternion.identity;  // Opcional: Restablece la rotación

        // Vuelve el objeto al pool
        objectPool.Enqueue(obj);
    }
}
