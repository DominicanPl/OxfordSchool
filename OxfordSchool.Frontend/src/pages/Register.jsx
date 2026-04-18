import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../services/api";

function Register() {
  const navigate = useNavigate();
  const [nombre, setNombre] = useState("");
  const [correo, setCorreo] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [rol, setRol] = useState("Estudiante");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const onSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);

    try {
      await api.post("/auth/register", { nombre, correo, contrasena, rol });
      setSuccess("Usuario registrado correctamente. Redirigiendo a login...");
      setTimeout(() => navigate("/login", { replace: true }), 1200);
    } catch (requestError) {
      const backendMessage = requestError?.response?.data?.message;
      setError(backendMessage || "No se pudo registrar el usuario.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main style={{ maxWidth: 420, margin: "60px auto", fontFamily: "Arial" }}>
      <h2>Oxford School - Registro</h2>
      <form onSubmit={onSubmit}>
        <input
          type="text"
          placeholder="Nombre completo"
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          required
          style={{ width: "100%", marginBottom: 10, padding: 10, boxSizing: "border-box" }}
        />
        <input
          type="email"
          placeholder="Correo"
          value={correo}
          onChange={(e) => setCorreo(e.target.value)}
          required
          style={{ width: "100%", marginBottom: 10, padding: 10, boxSizing: "border-box" }}
        />
        <input
          type="password"
          placeholder="Contraseña"
          value={contrasena}
          onChange={(e) => setContrasena(e.target.value)}
          required
          style={{ width: "100%", marginBottom: 10, padding: 10, boxSizing: "border-box" }}
        />

        <select
          value={rol}
          onChange={(e) => setRol(e.target.value)}
          style={{ width: "100%", marginBottom: 10, padding: 10, boxSizing: "border-box" }}
        >
          <option value="Estudiante">Estudiante</option>
          <option value="Docente">Docente</option>
        </select>

        <button disabled={loading} style={{ width: "100%", padding: 10 }}>
          {loading ? "Registrando..." : "Crear cuenta"}
        </button>

        {error && <p style={{ color: "crimson", marginTop: 10 }}>{error}</p>}
        {success && <p style={{ color: "green", marginTop: 10 }}>{success}</p>}
      </form>

      <p style={{ marginTop: 16 }}>
        ¿Ya tienes cuenta? <Link to="/login">Inicia sesión</Link>
      </p>
    </main>
  );
}

export default Register;
