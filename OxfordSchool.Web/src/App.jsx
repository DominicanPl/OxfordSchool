import { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

const API_URL = 'https://localhost:7040/api';

function App() {
  const [token, setToken] = useState(localStorage.getItem('token') || '');
  const [vista, setVista] = useState(token ? 'materias' : 'login');

  const [materiaSeleccionada, setMateriaSeleccionada] = useState(null);
  const [materias, setMaterias] = useState([]);
  const [tareas, setTareas] = useState([]);
  const [filtroMateria, setFiltroMateria] = useState('');

  async function cargarMaterias() {
    try {
      const resp = await axios.get(`${API_URL}/materias`);
      setMaterias(Array.isArray(resp.data) ? resp.data : []);
    } catch (err) {
      console.error(err);
      setMaterias([]);
    }
  }

  useEffect(() => {
    if (!token) {
      delete axios.defaults.headers.common.Authorization;
      localStorage.removeItem('token');
      return;
    }

    axios.defaults.headers.common.Authorization = `Bearer ${token}`;
    localStorage.setItem('token', token);

    const load = async () => {
      await cargarMaterias();
    };

    load();
  }, [token]);

  const handleLogin = async (e) => {
    e.preventDefault();

    try {
      const resp = await axios.post(`${API_URL}/auth/login`, {
        correo: e.target.correo.value,
        contrasena: e.target.contrasena.value
      });

      setToken(resp.data.token);
      setVista('materias');
    } catch {
      alert('Error de login, revisa credenciales.');
    }
  };

  const verTareasMateria = async (materia) => {
    setMateriaSeleccionada(materia);

    try {
      const resp = await axios.get(`${API_URL}/tareas?materiaId=${materia.id}`);
      const tareasApi = Array.isArray(resp.data) ? resp.data : [];

      if (tareasApi.length > 0) {
        setTareas(tareasApi);
      } else {
        setTareas([
          { id: 1, titulo: 'Asignación 1: Ecuaciones', descripcion: 'Subir ensayo' }
        ]);
      }

      setVista('tareas');
    } catch {
      setTareas([
        { id: 1, titulo: 'Asignación 1: Ecuaciones', descripcion: 'Subir ensayo' }
      ]);
      setVista('tareas');
    }
  };

  const enviarTarea = async (e, tareaId) => {
    e.preventDefault();

    try {
      await axios.post(`${API_URL}/entregas`, {
        tareaId,
        archivoAdjuntoUrl: 'https://documento.doc',
        comentarios: e.target.comentarios.value
      });

      alert('¡Tarea subida exitosamente!');
      e.target.reset();
    } catch {
      alert('Error al enviar.');
    }
  };

  const cerrarSesion = () => {
    setToken('');
    setVista('login');
    setMaterias([]);
    setTareas([]);
    setMateriaSeleccionada(null);
  };

  if (vista === 'login') {
    return (
      <div className="login-container">
        <h1>Oxford School - Portal</h1>
        <form onSubmit={handleLogin} className="login-form">
          <input
            name="correo"
            type="email"
            placeholder="Correo (ej: estudiante@oxford.edu)"
            required
          />
          <input
            name="contrasena"
            type="password"
            placeholder="Contraseña"
            required
          />
          <button type="submit">Iniciar sesión</button>
        </form>
      </div>
    );
  }

  const materiasFiltradas = materias.filter((m) =>
    (m.nombre ?? '').toLowerCase().includes(filtroMateria.toLowerCase())
  );

  return (
    <div className="dashboard-container">
      <nav>
        <h2>Bienvenido, Estudiante</h2>
        <button onClick={cerrarSesion}>Cerrar Sesión</button>
      </nav>

      {vista === 'materias' && (
        <div>
          <h3>Mis Materias</h3>

          <input
            type="text"
            placeholder="Buscar materia..."
            value={filtroMateria}
            onChange={(e) => setFiltroMateria(e.target.value)}
            style={{ marginBottom: '20px', padding: '5px' }}
          />

          <ul>
            <li
              onClick={() => verTareasMateria({ id: 1, nombre: 'Matemáticas' })}
              style={{ cursor: 'pointer', color: 'blue' }}
            >
              Matemáticas
            </li>

            {materiasFiltradas.map((m) => (
              <li
                key={m.id}
                onClick={() => verTareasMateria(m)}
                style={{ cursor: 'pointer' }}
              >
                {m.nombre} {m.profesorEncargado ? `- ${m.profesorEncargado}` : ''}
              </li>
            ))}
          </ul>
        </div>
      )}

      {vista === 'tareas' && (
        <div className="tareas-seccion">
          <button
            onClick={() => {
              setVista('materias');
              cargarMaterias();
            }}
          >
            {'<'} Volver a Materias
          </button>

          <h3>Tareas de: {materiaSeleccionada?.nombre}</h3>

          {tareas.length === 0 && <p>No hay tareas registradas para esta materia.</p>}

          {tareas.map((t) => (
            <div
              key={t.id}
              style={{ border: '1px solid #ccc', margin: '10px', padding: '10px' }}
            >
              <h4>{t.titulo || 'Asignación 1: Ecuaciones'}</h4>
              <p>{t.descripcion}</p>

              <form onSubmit={(e) => enviarTarea(e, t.id)}>
                <textarea
                  name="comentarios"
                  placeholder="Profe, aquí está mi entrega..."
                  required
                  style={{ width: '100%', height: '60px' }}
                />
                <br />
                <button type="submit" style={{ marginTop: '10px' }}>
                  Enviar Tarea
                </button>
              </form>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default App;
