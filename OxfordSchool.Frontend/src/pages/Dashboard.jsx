import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";

function Dashboard() {
  const navigate = useNavigate();
  const [tareas, setTareas] = useState([]);
  const usuario = JSON.parse(localStorage.getItem("usuario") || "{}");

  useEffect(() => {
    const cargarTareas = async () => {
      try {
        const { data } = await api.get("/tareas");
        setTareas(data);
      } catch {
        localStorage.clear();
        navigate("/login", { replace: true });
      }
    };

    cargarTareas();
  }, [navigate]);

  const logout = () => {
    localStorage.clear();
    navigate("/login", { replace: true });
  };

  return (
    <main style={{ maxWidth: 900, margin: "30px auto", fontFamily: "Arial" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h2>Dashboard</h2>
        <button onClick={logout}>Cerrar sesión</button>
      </div>

      <p>
        Bienvenido: <strong>{usuario.nombre}</strong> ({usuario.rol})
      </p>

      <h3>Tareas</h3>
      {tareas.length === 0 ? (
        <p>Sin tareas disponibles.</p>
      ) : (
        <ul>
          {tareas.map((tarea) => (
            <li key={tarea.id} style={{ marginBottom: 8 }}>
              <strong>{tarea.titulo}</strong> - {tarea.materiaNombre} - {new Date(tarea.fechaLimite).toLocaleDateString()}
              {tarea.estado ? ` - Estado: ${tarea.estado}` : ""}
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}

export default Dashboard;
