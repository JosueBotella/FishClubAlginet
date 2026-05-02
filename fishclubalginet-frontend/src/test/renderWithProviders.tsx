import type { ReactElement } from 'react';
import { render, type RenderOptions } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { Notifications } from '@mantine/notifications';

/**
 * Render helper que envuelve cualquier componente en los providers globales
 * que la app real usa (Mantine + Notifications). Sin esto, los componentes
 * que usen hooks de Mantine fallan en jsdom.
 */
export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>,
) {
  return render(ui, {
    wrapper: ({ children }) => (
      <MantineProvider defaultColorScheme="light">
        <Notifications />
        {children}
      </MantineProvider>
    ),
    ...options,
  });
}
