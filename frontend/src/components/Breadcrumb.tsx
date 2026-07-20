import { Link, useLocation } from 'react-router-dom';
import { buildBreadcrumbItems } from './breadcrumbItems';

/**
 * Breadcrumb de navegação reutilizável, usado no topo das páginas de
 * listagem e formulário (Carnes, Compradores, Pedidos, Dashboard). Deriva o
 * caminho a partir da rota atual (react-router-dom) — não recebe props, para
 * não duplicar a mesma informação de rota já expressa em `App.tsx`.
 */
export function Breadcrumb() {
  const location = useLocation();
  const items = buildBreadcrumbItems(location.pathname);

  if (items.length === 0) {
    return null;
  }

  return (
    <nav className="breadcrumb" aria-label="Caminho de navegação">
      {items.map((item, index) => {
        const isLast = index === items.length - 1;

        return (
          <span key={`${item.label}-${index}`} className="breadcrumb-item">
            {item.path && !isLast ? (
              <Link to={item.path}>{item.label}</Link>
            ) : (
              <span aria-current={isLast ? 'page' : undefined}>{item.label}</span>
            )}
            {!isLast && (
              <span className="breadcrumb-separator" aria-hidden="true">
                {' > '}
              </span>
            )}
          </span>
        );
      })}
    </nav>
  );
}
