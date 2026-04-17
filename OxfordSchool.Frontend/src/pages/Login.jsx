import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";

function Login() {
  const navigate = useNavigate();
  const [correo, setCorreo] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const onSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setLoading(true);

    try {
      const { data } = await api.post("/auth/login", { correo, contrasena });
      localStorage.setItem("token", data.token);
      localStorage.setItem("usuario", JSON.stringify(data.usuario));
      navigate("/dashboard", { replace: true });
    } catch {
      setError("Credenciales inválidas.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main style={{ maxWidth: 380, margin: "60px auto", fontFamily: "Arial" }}>
      <h2>Oxford School - Login</h2>
      <form onSubmit={onSubmit}>
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
        <button disabled={loading} style={{ width: "100%", padding: 10 }}>
          {loading ? "Ingresando..." : "Ingresar"}
        </button>
        {error && <p style={{ color: "crimson", marginTop: 10 }}>{error}</p>}
      </form>
    </main>
  );
}

export default Login;
