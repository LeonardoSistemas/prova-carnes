import { useState } from 'react';
import { NavLink } from 'react-router-dom';

const linkClassName = ({ isActive }: { isActive: boolean }) => (isActive ? 'nav-link active' : 'nav-link');

export function Sidebar() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const closeMenu = () => setIsMenuOpen(false);

  return (
    <>
      {/* Botão hamburger — visível apenas em mobile (<768px) */}
      <button
        className="sidebar-hamburger"
        onClick={() => setIsMenuOpen(!isMenuOpen)}
        aria-label="Abrir menu"
        title="Abrir menu"
      >
        <span></span>
        <span></span>
        <span></span>
      </button>

      {/* Overlay — visível apenas em mobile quando menu está aberto */}
      {isMenuOpen && <div className="sidebar-overlay" onClick={closeMenu}></div>}

      {/* Sidebar — sempre visível em desktop (>=768px), overlay em mobile quando isMenuOpen=true */}
      <nav className={`sidebar ${isMenuOpen ? 'sidebar--mobile-open' : ''}`}>
        <span className="sidebar-brand">Prova Carnes</span>
        <NavLink to="/dashboard" className={linkClassName} onClick={closeMenu}>
          Dashboard
        </NavLink>
        <NavLink to="/carnes" className={linkClassName} onClick={closeMenu}>
          Carnes
        </NavLink>
        <NavLink to="/compradores" className={linkClassName} onClick={closeMenu}>
          Compradores
        </NavLink>
        <NavLink to="/pedidos" className={linkClassName} onClick={closeMenu}>
          Pedidos
        </NavLink>
      </nav>
    </>
  );
}
