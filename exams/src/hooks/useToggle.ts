import { useEffect, useState } from 'react';

export const useToggle = (defaultValue = false) => {
  const [isOpen, setIsOpen] = useState(defaultValue);
  useEffect(() => {
    if (defaultValue) setIsOpen(defaultValue);
  }, [defaultValue]);
  const toggle = () => setIsOpen(!isOpen);
  const close = () => setIsOpen(false);
  const open = () => setIsOpen(true);
  return { isOpen, toggle, close, open };
};
