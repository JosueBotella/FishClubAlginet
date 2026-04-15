import { useAuth } from '../../hooks';

export default function HomePage() {
  const { user, logout } = useAuth();

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Bienvenido, {user?.email}</h1>
      <p>Roles: {user?.roles.join(', ') || 'Ninguno'}</p>
      <button onClick={logout}>Cerrar sesión</button>
    </div>
  );
}
