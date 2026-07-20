import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { Sidebar } from './components/Sidebar';
import { CarneFormPage } from './pages/carnes/CarneFormPage';
import { CarnesPage } from './pages/carnes/CarnesPage';
import { CompradorFormPage } from './pages/compradores/CompradorFormPage';
import { CompradoresPage } from './pages/compradores/CompradoresPage';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { PedidoFormPage } from './pages/pedidos/PedidoFormPage';
import { PedidosListPage } from './pages/pedidos/PedidosListPage';

function App() {
  const location = useLocation();

  return (
    <div className="app-shell">
      <Sidebar />
      <main className="app-content">
        {/* `key={pathname}` remonta o wrapper a cada troca de rota, o que
            retrigger a animação `page-fade-in` (CSS puro, ver index.css) —
            fade sutil de conteúdo sem precisar de nenhuma lib de transição. */}
        <div key={location.pathname} className="page-fade">
          <Routes>
            <Route path="/" element={<Navigate to="/carnes" replace />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/carnes" element={<CarnesPage />} />
            <Route path="/carnes/novo" element={<CarneFormPage />} />
            <Route path="/carnes/:id/editar" element={<CarneFormPage />} />
            <Route path="/compradores" element={<CompradoresPage />} />
            <Route path="/compradores/novo" element={<CompradorFormPage />} />
            <Route path="/compradores/:id/editar" element={<CompradorFormPage />} />
            <Route path="/pedidos" element={<PedidosListPage />} />
            <Route path="/pedidos/novo" element={<PedidoFormPage />} />
            <Route path="/pedidos/:id/editar" element={<PedidoFormPage />} />
            <Route path="*" element={<Navigate to="/carnes" replace />} />
          </Routes>
        </div>
      </main>
    </div>
  );
}

export default App;
